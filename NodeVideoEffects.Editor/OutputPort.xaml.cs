using NodeVideoEffects.Type;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for OutputPort.xaml
    /// </summary>
    public partial class OutputPort : UserControl
    {
        Output _output;
        string _id;
        int _index;
        Editor editor;
        Point startPos;

        private bool isMouseDown = false;
        public OutputPort(Output output, string id, int index)
        {
            InitializeComponent();
            _output = output;
            _id = id;
            _index = index;
            portName.Content = output.Name;
            port.Fill = new SolidColorBrush(output.Color);
            ToolTip = new();
            ToolTipOpening += OutputPort_ToolTipOpening;

            Loaded += (s, e) =>
            {
                editor = FindParent<Editor>(this);
            };
        }

        public System.Type Type => _output.Type;
        public string ID => _id;
        public int Index => _index;

        private void OutputPort_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            Task<object> @object = NodesManager.GetOutputValue(_id, _index);
            @object.Wait();
            ToolTip = @object.Result?.ToString() ?? "(Null)";
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private void port_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            port.CaptureMouse();
            startPos = e.GetPosition(editor);
            editor.PreviewConnection(startPos, startPos);
            e.Handled = true;
        }

        private void port_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
                editor.PreviewConnection(startPos, e.GetPosition(editor));
            e.Handled = true;
        }

        private void port_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;

            editor.RemovePreviewConnection();

            // Get the control under the mouse pointer
            Point position = e.GetPosition(editor);
            HitTestResult result = VisualTreeHelper.HitTest(editor, position);

            if (result != null)
            {
                var element = result.VisualHit as FrameworkElement;

                if (element != null)
                {
                    if (SetConnectionToInputPort(element, port.PointToScreen(new(5, 5)), position))
                        editor.OnNodesUpdated();
                }
            }
            port.ReleaseMouseCapture();
            e.Handled = true;
        }

        private bool SetConnectionToInputPort(DependencyObject element, Point pos1, Point pos2)
        {
            InputPort? inputPort = FindParent<InputPort>(element);
            if (inputPort != null)
            {
                if (inputPort.Type.IsAssignableFrom(Type))
                {
                    if (NodesManager.CheckConnection(inputPort.ID, _id))
                    {
                        inputPort.SetConnection(_id, _index);
                        _output.AddConnection(inputPort.ID, inputPort.Index);
                        editor.AddConnector(pos1, inputPort.port.PointToScreen(new(5, 5)),
                        ((SolidColorBrush)port.Fill).Color,
                        ((SolidColorBrush)inputPort.port.Fill).Color,
                        new(inputPort.ID, inputPort.Index), new(_id, _index));
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddConnection(string id, int index) => _output.AddConnection(id, index);

        public void RemoveAllConnection()
        {
            List<Connection> connections = new List<Connection>(_output.Connection);
            connections.ForEach(connection => NodesManager.GetNode(connection.id)?.RemoveInputConnection(connection.index));
            _output.Connection.Clear();
            editor.RemoveOutputConnector(_id, _index);
        }

        private void port_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveAllConnection();
            editor.OnNodesUpdated();
        }
    }
}
