using NodeVideoEffects.Core;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Editor;

/// <summary>
/// Interaction logic for Node.xaml
/// </summary>
public partial class Node
{
    private readonly string _name;

    private bool _isDragging;
    private bool _isClicking;
    private Canvas? _wrapperCanvas;
    private Editor? _editor;
    private Point _lastPos;

    public string Id { get; }
    public Type Type { get; }
    public List<object?> Values { get; } = [];

    public event PropertyChangedEventHandler ValueChanged = delegate { };

    public Node(NodeLogic node)
    {
        InitializeComponent();
        NodeName.Content = _name = node.Name;
        Id = node.Id;
        CategorySign.Fill = new SolidColorBrush(node.Color);
        Type = node.GetType();
        {
            var index = 0;
            foreach (var output in node.Outputs)
            {
                OutputsPanel.Children.Add(new OutputPort(output, node.Id, index));
                index++;
            }
        }
        {
            var index = 0;
            foreach (var input in node.Inputs)
            {
                var inputPort = new InputPort(input, node.Id, index);
                InputsPanel.Children.Add(inputPort);
                Values.Add(input.Value);
                if (input.PortInfo.Id != "")
                    inputPort.Loaded += (_, _) => inputPort.PortControl.Visibility = Visibility.Hidden;
                input.PropertyChanged += (_, _) => ValueChanged(this, new PropertyChangedEventArgs(_name));
                index++;
            }
        }
        Loaded += (_, _) =>
        {
            _wrapperCanvas = FindParent<Canvas>(this);
            _editor = FindParent<Editor>(this);
        };
    }

    private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
    {
        if (child == null)
            return null;
        var parentObject = VisualTreeHelper.GetParent(child);

        return parentObject switch
        {
            null => null,
            T parent => parent,
            _ => FindParent<T>(parentObject)
        };
    }

    internal Point GetPortPoint(PortType type, int index)
    {
        return type == PortType.Input
            ? (InputsPanel.Children[index] as InputPort)!.Port.PointToScreen(new Point(5, 5))
            : (OutputsPanel.Children[index] as OutputPort)!.Port.PointToScreen(new Point(5, 5));
    }

    internal enum PortType
    {
        Input,
        Output
    }

    #region Move node

    public event PropertyChangedEventHandler Moved = delegate { };

    private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Node node) return;
        _isDragging = false;
        _isClicking = true;
        _lastPos = e.GetPosition(_wrapperCanvas);
        _editor?.RestoreChild(this);
        node.CaptureMouse();
        e.Handled = true;
    }

    private void Node_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Node || !_isClicking) return;
        if (!_isDragging)
            if (Math.Abs(_lastPos.X - e.GetPosition(_wrapperCanvas).X) > SystemParameters.MinimumHorizontalDragDistance
                || Math.Abs(_lastPos.Y - e.GetPosition(_wrapperCanvas).Y) >
                SystemParameters.MinimumHorizontalDragDistance)
                _isDragging = true;

        if (!_isDragging) return;
        var p = e.GetPosition(_wrapperCanvas);
        _editor?.MoveNode(this, p.X - _lastPos.X, p.Y - _lastPos.Y);
        _lastPos = p;
        e.Handled = true;
    }

    private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isClicking || sender is not Node node) return;
        if (!_isDragging)
            _editor?.ToggleSelection(this, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
        _isClicking = false;
        _isDragging = false;
        node.ReleaseMouseCapture();
        _editor?.SubmitMoving();
        e.Handled = true;
    }

    internal void Move(double dx, double dy)
    {
        Canvas.SetLeft(this, Canvas.GetLeft(this) + dx);
        Canvas.SetTop(this, Canvas.GetTop(this) + dy);
        _editor?.MoveConnector(Id, new Point(dx, dy));
    }

    internal void SubmitMoving()
    {
        Moved(this, new PropertyChangedEventArgs(_name));
    }

    #endregion
}