using NodeVideoEffects.Control;
using NodeVideoEffects.Core;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Timer = System.Threading.Timer;

namespace NodeVideoEffects.Editor;

/// <summary>
/// Interaction logic for OutputPort.xaml
/// </summary>
public partial class InputPort
{
    private readonly IControl _control;
    private readonly Input _input;

    private Editor? _editor;
    private Point _startPos;

    private bool _isMouseDown;

    private DateTime _lastExecution = DateTime.MinValue;
    private Timer? _debounceTimer;
    private const int DebounceInterval = 200; // [ms]

    public InputPort(Input input, string id, int index)
    {
        InitializeComponent();
        _input = input;
        Id = id;
        Index = index;
        PortName.Content = input.Name;
        Port.Fill = new SolidColorBrush(input.Color);
        _control = input.Control;
        _control.PropertyChanged += (_, _) =>
        {
            // Debounce the property changed event
            var now = DateTime.UtcNow;
            if ((now - _lastExecution).TotalMilliseconds >= DebounceInterval)
            {
                // Execute immediately
                _lastExecution = now;
                if (_debounceTimer != null)
                {
                    _debounceTimer.Dispose();
                    _debounceTimer = null;
                }

                Dispatcher.Invoke(OnControlPropertyChanged);
            }

            if (_debounceTimer != null)
                // Reset the timer
                _debounceTimer.Change(DebounceInterval, Timeout.Infinite);
            else
                // Start the timer
                _debounceTimer = new Timer(_ =>
                {
                    Dispatcher.Invoke(OnControlPropertyChanged);
                    _lastExecution = DateTime.UtcNow;
                    _debounceTimer?.Dispose();
                    _debounceTimer = null;
                }, null, DebounceInterval, Timeout.Infinite);
        };

        PortControl.Content = _control;
        Loaded += (_, _) => { _editor = FindParent<Editor>(this); };
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        //get parent item
        var parentObject = VisualTreeHelper.GetParent(child);

        return parentObject switch
        {
            //we've reached the end of the tree
            null => null,
            //check if the parent matches the type we're looking for
            T parent => parent,
            _ => FindParent<T>(parentObject)
        };
    }

    public object? Value => _control.Value;

    public string Id { get; }

    public int Index { get; }

    public Type Type => _input.Type;

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    private void OnControlPropertyChanged()
    {
        _input.Value = _control.Value;
        PropertyChanged(this, new PropertyChangedEventArgs(Name));
    }

    public void SetConnection(string id, int index)
    {
        _input.SetConnection(Id, Index, id, index);
        PortControl.Visibility = Visibility.Hidden;
    }

    public void RemoveConnection()
    {
        _input.RemoveConnection(Id, Index);
        _editor?.RemoveInputConnector(Id, Index);
        PortControl.Visibility = Visibility.Visible;
    }

    private void Port_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        RemoveConnection();
        _editor?.OnNodesUpdated();
    }

    private void port_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = true;
        Port.CaptureMouse();
        _startPos = e.GetPosition(_editor);
        _editor?.PreviewConnection(_startPos, _startPos);
        e.Handled = true;
    }

    private void port_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isMouseDown)
            _editor?.PreviewConnection(_startPos, e.GetPosition(_editor));
        e.Handled = true;
    }

    private void port_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = false;

        _editor?.RemovePreviewConnection();

        // Get the control under the mouse pointer
        var position = e.GetPosition(_editor);
        if (_editor != null)
        {
            var result = VisualTreeHelper.HitTest(_editor, position);

            if (result?.VisualHit is FrameworkElement element)
                AddConnectionToOutputPort(element, Port.PointToScreen(new Point(5, 5)));
        }

        Port.ReleaseMouseCapture();
        e.Handled = true;
    }

    private void AddConnectionToOutputPort(DependencyObject element, Point pos1)
    {
        var outputPort = FindParent<OutputPort>(element);
        if (outputPort == null
            || !outputPort.Type.IsAssignableFrom(Type)
            || !NodesManager.CheckConnection(Id, outputPort.Id)) return;
        SetConnection(outputPort.Id, outputPort.Index);
        outputPort.AddConnection(Id, Index);
        _editor?.AddConnector(outputPort.Port.PointToScreen(new Point(5, 5)),
            pos1,
            ((SolidColorBrush)outputPort.Port.Fill).Color,
            ((SolidColorBrush)Port.Fill).Color,
            new PortInfo(Id, Index),
            new PortInfo(outputPort.Id, outputPort.Index));
    }
}