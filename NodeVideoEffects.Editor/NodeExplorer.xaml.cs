using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock;
using AvalonDock.Layout;
using NodeVideoEffects.Core;
using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Editor;

/// <summary>
/// Interaction logic for NodeExplorer.xaml
/// </summary>
public partial class NodeExplorer
{
    private Type? _type;

    public NodeExplorer()
    {
        InitializeComponent();

        Dispatcher.InvokeAsync(() =>
        {
            var baseType = typeof(NodeLogic);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(baseType) &&
                                t != typeof(InputNode) && t != typeof(OutputNode));

                foreach (var type in types) AddTypeToExplorerRoot(type);
            }

            DataContext = this;
            return;

            void AddTypeToExplorerRoot(Type type)
            {
                NodeLogic? obj;
                try
                {
                    obj = Activator.CreateInstance(type, "") as NodeLogic;
                }
                catch
                {
                    obj = Activator.CreateInstance(type, []) as NodeLogic;
                }

                var category = obj?.Category?.Split('/') ?? [Text_UI.NoCategory];
                if ((type.Namespace?.Split('.') ?? [])[0] == "NodeVideoEffects")
                {
                    var temp = new string[category.Length + 1];
                    temp[0] = Text_UI.Accessory;
                    Array.Copy(category, 0, temp, 1, category.Length);
                    category = temp;
                }
                else
                {
                    var temp = new string[category.Length + 1];
                    temp[0] = Text_UI.Extension;
                    Array.Copy(category, 0, temp, 1, category.Length);
                    category = temp;
                }

                NodesTree? currentNode = null;

                foreach (var ns in category)
                {
                    var node = currentNode?.Children?.FirstOrDefault(n => n.Text == ns)
                               ?? Root.FirstOrDefault(n => n.Text == ns);

                    if (node == null)
                    {
                        node = new NodesTree { Text = ns };
                        if (currentNode == null)
                            Root.Add(node);
                        else
                            currentNode.Add(node);
                    }

                    currentNode = node;
                }

                var typeNode = new NodesTree { Text = obj?.Name ?? type.Name, Type = type };
                currentNode?.Add(typeNode);
            }
        });
    }

    public ObservableCollection<NodesTree> Root { get; } = [];

    public required DockingManager DockingManager { init; get; }

    private static void TryHitTestFloatingElement<T>(DockingManager? dockManager,
        Point screenPoint,
        out T? hitElement)
        where T : FrameworkElement
    {
        hitElement = null;

        if (dockManager?.Layout is not { } layoutRoot) return;

        foreach (var anchorable in layoutRoot.Descendents().OfType<LayoutAnchorable>())
        {
            if (anchorable.Content is not FrameworkElement { IsVisible: true } fe) continue;
            foreach (var element in FindVisualChildren<T>(fe))
            {
                if (!element.IsVisible || element.ActualWidth <= 0 || element.ActualHeight <= 0)
                    continue;

                var topLeft = element.PointToScreen(new Point(0, 0));
                var bottomRight = element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight));

                if (!(screenPoint.X >= topLeft.X) || !(screenPoint.X <= bottomRight.X) ||
                    !(screenPoint.Y >= topLeft.Y) || !(screenPoint.Y <= bottomRight.Y)) continue;
                hitElement = element;
                return;
            }
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
    {
        if (depObj == null)
            yield break;

        var count = VisualTreeHelper.GetChildrenCount(depObj);

        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            if (child is T t)
                yield return t;

            foreach (var childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }

    private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Cursor = Cursors.Arrow;
        if (sender is not StackPanel obj) return;
        if (obj.DataContext is not NodesTree dataContext) return;
        _type = dataContext.Type;
        if (_type != null)
            obj.CaptureMouse();
    }

    private void Item_MouseMove(object sender, MouseEventArgs e)
    {
        if (_type != null && sender is StackPanel)
        {
            var position = PointToScreen(e.GetPosition(this));
            TryHitTestFloatingElement<Editor>(
                DockingManager,
                position,
                out var editor);
            Cursor = editor != null ? Cursors.Arrow : Cursors.No;
        }

        e.Handled = true;
    }

    private void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Cursor = Cursors.Arrow;
        if (_type != null && sender is StackPanel obj)
        {
            var position = PointToScreen(e.GetPosition(this));
            TryHitTestFloatingElement<Editor>(
                DockingManager,
                position,
                out var editor);
            if (editor != null)
            {
                NodeLogic? node = null;
                try
                {
                    try
                    {
                        node = Activator.CreateInstance(_type, editor.ItemId) as NodeLogic;
                    }
                    catch (MissingMethodException)
                    {
                        node = Activator.CreateInstance(_type, []) as NodeLogic;
                    }
                }
                catch (Exception exception)
                {
                    Logger.Write(LogLevel.Error, exception.Message, exception);
                }

                if (node != null)
                {
                    node.Id = editor.ItemId + "-" + Guid.NewGuid().ToString("N");
                    for (var i = 0; i < (node.Inputs?.Length ?? 0); i++)
                        node.SetInputConnection(i, new PortInfo());
                    NodesManager.AddNode(node.Id, node);
                    editor.AddChildren(new Node(node),
                        editor.ConvertToTransform(e.GetPosition(editor)).X,
                        editor.ConvertToTransform(e.GetPosition(editor)).Y);
                    editor.OnNodesUpdated();
                }
            }

            obj.ReleaseMouseCapture();
            _type = null;
        }

        e.Handled = true;
    }
}

public class NodesTree : INotifyPropertyChanged
{
    private readonly string _text = "";
    private readonly Type? _type;
    private ObservableCollection<NodesTree>? _children;
    private bool _isExpanded = true;
    private NodesTree? _parent;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            OnPropertyChanged("IsExpanded");
        }
    }

    public string Text
    {
        get => _text;
        init
        {
            _text = value;
            OnPropertyChanged("Text");
        }
    }

    public Type? Type
    {
        get => _type;
        init
        {
            _type = value;
            OnPropertyChanged("Text");
        }
    }

    public NodesTree? Parent
    {
        get => _parent;
        set
        {
            _parent = value;
            OnPropertyChanged("Parent");
        }
    }

    public ObservableCollection<NodesTree>? Children
    {
        get => _children;
        set
        {
            _children = value;
            OnPropertyChanged("Children");
        }
    }

    public Brush Color
    {
        get
        {
            Color color;
            try
            {
                NodeLogic? node;
                try
                {
                    if (Type != null)
                    {
                        node = Activator.CreateInstance(Type, "") as NodeLogic;
                        if (node != null) color = node.Color;
                    }
                }
                catch
                {
                    if (Type != null)
                    {
                        node = Activator.CreateInstance(Type, []) as NodeLogic;
                        if (node != null) color = node.Color;
                    }
                }
            }
            catch
            {
                color = SystemColors.GrayTextColor;
            }

            return new SolidColorBrush(color);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void Add(NodesTree child)
    {
        Children ??= [];
        child.Parent = this;
        Children.Add(child);
    }
}

public static class TreeViewItemExtensions
{
    public static int GetDepth(this TreeViewItem item)
    {
        while (GetParent(item) is { } parent) return GetDepth(parent) + 1;
        return 0;
    }

    private static TreeViewItem? GetParent(TreeViewItem item)
    {
        var parent = VisualTreeHelper.GetParent(item);
        while (parent is not (TreeViewItem or TreeView))
            if (parent != null)
                parent = VisualTreeHelper.GetParent(parent);
        return parent as TreeViewItem;
    }
}

public class LeftMarginMultiplierConverter : IValueConverter
{
    public double Length { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not TreeViewItem item ? new Thickness(0) : new Thickness(Length * item.GetDepth(), 0, 0, 0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Thickness thickness ? thickness.Left / Length : 0;
    }
}

public class WidthToVisibilityConverter : IValueConverter
{
    public double Threshold { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double width) return width < Threshold ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
            return visibility == Visibility.Visible ? Threshold - 1 : Threshold + 1;
        return Threshold + 1;
    }
}