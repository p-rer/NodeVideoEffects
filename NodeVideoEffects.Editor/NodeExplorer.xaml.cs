using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for NodeExplorer.xaml
    /// </summary>
    public partial class NodeExplorer
    {
        public ObservableCollection<NodesTree> Root { get; } = [];
        private System.Type? _type;
        private Window? _window;
        public NodeExplorer()
        {
            InitializeComponent();

            Dispatcher.InvokeAsync(() =>
            {
                var baseType = typeof(INode);

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(baseType));

                    foreach (var type in types)
                    {
                        AddTypeToExplorerRoot(type);
                    }
                }

                DataContext = this;
                return;

                void AddTypeToExplorerRoot(System.Type type)
                {
                    INode? obj;
                    try
                    {
                        obj = Activator.CreateInstance(type, [""]) as INode;
                    }
                    catch
                    {
                        obj = Activator.CreateInstance(type, []) as INode;
                    }

                    var category = obj?.Category?.Split('/') ?? ["(No Category)"];
                    if ((type.Namespace?.Split('.') ?? [])[0] == "NodeVideoEffects")
                    {
                        var temp = new string[category.Length + 1];
                        temp[0] = "Accessory";
                        Array.Copy(category, 0, temp, 1, category.Length);
                        category = temp;
                    }
                    else
                    {
                        var temp = new string[category.Length + 1];
                        temp[0] = "Expansion";
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
                            {
                                Root.Add(node);
                            }
                            else
                            {
                                currentNode.Add(node);
                            }
                        }

                        currentNode = node;
                    }

                    var typeNode = new NodesTree { Text = obj?.Name ?? type.Name, Type = type };
                    currentNode?.Add(typeNode);
                }
            });

            Loaded += (_, _) =>
            {
                _window = FindParent<Window>(this);
            };
        }

        private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            if (child == null) return null;
            var parentObject = VisualTreeHelper.GetParent(child);

            return parentObject switch
            {
                null => null,
                T parent => parent,
                _ => FindParent<T>(parentObject)
            };
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock textBlock) return;
            if (textBlock.DataContext is not NodesTree dataContext) return;
            if (dataContext.Type == typeof(InputNode) || dataContext.Type == typeof(OutputNode))
                return;
            _type = dataContext.Type;
            if (_type != null)
                textBlock.CaptureMouse();
        }

        private void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock)
            {
                if (_type != null)
                {
                    var position = e.GetPosition(_window);
                    if (_window != null)
                    {
                        var result = VisualTreeHelper.HitTest(_window, position);

                        if (result?.VisualHit is FrameworkElement element)
                        {
                            var editor = FindParent<Editor>(element);
                            Cursor = editor != null ? Cursors.Arrow : Cursors.No;
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_type != null)
            {
                if (sender is TextBlock textBlock)
                {
                    if (_type != null)
                    {
                        var position = e.GetPosition(_window);
                        if (_window != null)
                        {
                            var result = VisualTreeHelper.HitTest(_window, position);

                            if (result?.VisualHit is FrameworkElement element)
                            {
                                var editor = FindParent<Editor>(element);
                                if (editor != null)
                                {
                                    INode? node;
                                    try
                                    {
                                        node = Activator.CreateInstance(_type, editor.ItemId) as INode;
                                    }
                                    catch
                                    {
                                        node = Activator.CreateInstance(_type, []) as INode;
                                    }
                                    if (node != null)
                                    {
                                        node.Id = editor.ItemId + "-" + Guid.NewGuid().ToString("N");
                                        for (var i = 0; i < (node.Inputs?.Length ?? 0); i++)
                                        {
                                            node.SetInputConnection(i, new Connection());
                                        }
                                        NodesManager.AddNode(node.Id, node);
                                        editor.AddChildren(new Node(node), editor.ConvertToTransform(e.GetPosition(editor)).X, editor.ConvertToTransform(e.GetPosition(editor)).Y);
                                        editor.OnNodesUpdated();
                                    }
                                }
                            }
                        }
                    }
                    textBlock.ReleaseMouseCapture();
                }
                _type = null;
            }
            e.Handled = true;
        }
    }

    public class NodesTree : INotifyPropertyChanged
    {
        private bool _isExpanded = true;
        private readonly string _text = "";
        private readonly System.Type? _type;
        private NodesTree? _parent;
        private ObservableCollection<NodesTree>? _children;

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged("IsExpanded"); }
        }

        public string Text
        {
            get => _text;
            init { _text = value; OnPropertyChanged("Text"); }
        }

        public System.Type? Type
        {
            get => _type;
            init { _type = value; OnPropertyChanged("Text"); }
        }

        public NodesTree? Parent
        {
            get => _parent;
            set { _parent = value; OnPropertyChanged("Parent"); }
        }

        public ObservableCollection<NodesTree>? Children
        {
            get => _children;
            set { _children = value; OnPropertyChanged("Children"); }
        }

        public Brush Color
        {
            get
            {
                Color color;
                try
                {
                    INode? node;
                    try
                    {
                        if (Type != null)
                        {
                            node = Activator.CreateInstance(Type, "") as INode;
                            if (node != null) color = node.Color;
                        }
                    }
                    catch
                    {
                        if (Type != null)
                        {
                            node = Activator.CreateInstance(Type, []) as INode;
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
            while (GetParent(item) is { } parent)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private static TreeViewItem? GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (parent is not (TreeViewItem or TreeView))
            {
                if (parent != null) parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }

    public class LeftMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture) =>
            value is not TreeViewItem item ? new Thickness(0) : new Thickness(Length * item.GetDepth(), 0, 0, 0);

        public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture) =>
            value is Thickness thickness ? thickness.Left / Length : 0;
    }

    public class WidthToVisibilityConverter : IValueConverter
    {
        public double Threshold { get; set; }

        public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return width < Threshold ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible ? Threshold - 1 : Threshold + 1;
            return Threshold + 1;
        }
    }
}
