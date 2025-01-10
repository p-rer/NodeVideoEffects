using NodeVideoEffects.Control;
using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for OutputPort.xaml
    /// </summary>
    public partial class InputPort
    {
        private readonly IControl? _control;
        private readonly Input _input;

        private Editor? _editor;
        private Point _startPos;

        private bool _isMouseDown;

        public InputPort(Input input, string id, int index)
        {
            InitializeComponent();
            _input = input;
            Id = id;
            Index = index;
            PortName.Content = input.Name;
            _control = input.Control;
            Port.Fill = new SolidColorBrush(input.Color);
            (_control as System.Windows.Controls.Control)!.Loaded += (_, _) => { _control.PropertyChanged += OnControlPropertyChanged; };
            PortControl.Content = _control;
            Loaded += (_, _) =>
            {
                _editor = FindParent<Editor>(this);
            };
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

        public object? Value => _control?.Value;

        public string Id { get; }

        public int Index { get; }

        public System.Type Type => _input.Type;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnControlPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _input.Value = _control?.Value;
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
            Point position = e.GetPosition(_editor);
            if (_editor != null)
            {
                var result = VisualTreeHelper.HitTest(_editor, position);

                if (result?.VisualHit is FrameworkElement element)
                {
                    if (AddConnectionToOutputPort(element, Port.PointToScreen(new Point(5, 5))))
                        _editor.OnNodesUpdated();
                }
            }

            Port.ReleaseMouseCapture();
            e.Handled = true;
        }

        private bool AddConnectionToOutputPort(DependencyObject element, Point pos1)
        {
            var outputPort = FindParent<OutputPort>(element);
            if (outputPort == null) return false;
            if (!outputPort.Type.IsAssignableFrom(Type)) return false;
            if (!NodesManager.CheckConnection(Id, outputPort.Id)) return false;
            SetConnection(outputPort.Id, outputPort.Index);
            outputPort.AddConnection(Id, Index);
            _editor?.AddConnector(outputPort.Port.PointToScreen(new Point(5, 5)),
                pos1,
                ((SolidColorBrush)outputPort.Port.Fill).Color,
                ((SolidColorBrush)Port.Fill).Color,
                new Connection(Id, Index),
                new Connection(outputPort.Id, outputPort.Index));

            return true;
        }
    }
}
