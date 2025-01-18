using NodeVideoEffects.Type;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for OutputPort.xaml
    /// </summary>
    public partial class OutputPort
    {
        private readonly Output _output;
        private Editor? _editor;
        private Point _startPos;

        private bool _isMouseDown;
        public OutputPort(Output output, string id, int index)
        {
            InitializeComponent();
            _output = output;
            Id = id;
            Index = index;
            PortName.Content = output.Name;
            Port.Fill = new SolidColorBrush(output.Color);
            ToolTip = new object();
            ToolTipOpening += OutputPort_ToolTipOpening;

            Loaded += (_, _) =>
            {
                _editor = FindParent<Editor>(this);
            };
        }

        public System.Type Type => _output.Type;
        public string Id { get; }

        public int Index { get; }

        private async void OutputPort_ToolTipOpening(object sender, ToolTipEventArgs args)
        {
            try
            {
                var @object = await TaskTracker.RunTrackedTask(() => NodesManager.GetOutputValue(Id, Index));
                ToolTip = @object?.ToString() ?? "(Null)";
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Error, e.Message);
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            return parentObject switch
            {
                null => null,
                T parent => parent,
                _ => FindParent<T>(parentObject)
            };
        }

        private void port_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            Port.CaptureMouse();
            _startPos = e.GetPosition(_editor);
            _editor!.PreviewConnection(_startPos, _startPos);
            e.Handled = true;
        }

        private void port_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
                _editor!.PreviewConnection(_startPos, e.GetPosition(_editor));
            e.Handled = true;
        }

        private void port_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;

            _editor!.RemovePreviewConnection();

            // Get the control under the mouse pointer
            var result = VisualTreeHelper.HitTest(_editor!, e.GetPosition(_editor));

            if (result?.VisualHit is FrameworkElement element)
            {
                if (SetConnectionToInputPort(element, Port.PointToScreen(new Point(5, 5))))
                    _editor!.OnNodesUpdated();
            }
            Port.ReleaseMouseCapture();
            e.Handled = true;
        }

        private bool SetConnectionToInputPort(DependencyObject element, Point pos1)
        {
            var inputPort = FindParent<InputPort>(element);
            if (inputPort == null) return false;
            if (!inputPort.Type.IsAssignableFrom(Type)) return false;
            if (!NodesManager.CheckConnection(inputPort.Id, Id)) return false;
            inputPort.SetConnection(Id, Index);
            _output.AddConnection(inputPort.Id, inputPort.Index);
            _editor!.AddConnector(pos1, inputPort.Port.PointToScreen(new Point(5, 5)),
                ((SolidColorBrush)Port.Fill).Color,
                ((SolidColorBrush)inputPort.Port.Fill).Color,
                new Connection(inputPort.Id, inputPort.Index), new Connection(Id, Index));
            return true;
        }

        public void AddConnection(string id, int index) => _output.AddConnection(id, index);

        public void RemoveAllConnection()
        {
            var connections = new List<Connection>(_output.Connection);
            connections.ForEach(connection => NodesManager.GetNode(connection.Id)?.RemoveInputConnection(connection.Index));
            _output.Connection.Clear();
            _editor!.RemoveOutputConnector(Id, Index);
        }

        private void port_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveAllConnection();
            _editor!.OnNodesUpdated();
        }
    }
}
