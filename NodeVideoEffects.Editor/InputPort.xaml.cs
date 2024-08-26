using NodeVideoEffects.Control;
using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for OutputPort.xaml
    /// </summary>
    public partial class InputPort : UserControl
    {
        private IControl? control;
        private Input _input;
        private string _id;
        private int _index;

        Editor editor;
        Point startPos;

        private bool isMouseDown = false;

        public InputPort(Input input, string id, int index)
        {
            InitializeComponent();
            _input = input;
            _id = id;
            _index = index;
            portName.Content = input.Name;
            control = input.Control;
            port.Fill = new SolidColorBrush(input.Color);
            if (control is System.Windows.Controls.Control)
                ((System.Windows.Controls.Control)control).Loaded += (s, e) => { control.PropertyChanged += OnControlPropertyChanged; };
            portControl.Content = control;
            Loaded += (s, e) =>
            {
                editor = FindParent<Editor>(this);
            };
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public object? Value
        {
            get => control?.Value;
            set
            {
                if (control != null)
                    control.Value = value;
            }
        }

        public string ID => _id;
        public int Index => _index;
        public System.Type Type => _input.Type;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnControlPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _input.Value = control.Value;
            PropertyChanged?.Invoke(this, new(Name));
        }

        public void SetConnection(string id, int index)
        {
            _input.SetConnection(_id, _index, id, index);
            portControl.Visibility = Visibility.Hidden;
        }

        public void RemoveConnection()
        {
            _input.RemoveConnection(_id, _index);
            editor.RemoveConnector(_id, _index);
            portControl.Visibility = Visibility.Visible;
        }

        private void Port_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveConnection();
        }

        private void port_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void port_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                    AddConnectionToOutputPort(element, port.PointToScreen(new(5, 5)), position);
                }
            }
            port.ReleaseMouseCapture();
            e.Handled = true;
        }

        private bool AddConnectionToOutputPort(DependencyObject element, Point pos1, Point pos2)
        {
            OutputPort? outputPort = FindParent<OutputPort>(element);
            if (outputPort != null)
            {
                if (outputPort.Type.IsAssignableFrom(Type))
                {
                    if (NodesManager.CheckConnection(_id, outputPort.ID))
                    {
                        SetConnection(outputPort.ID, outputPort.Index);
                        outputPort.AddConnection(ID, Index);
                        editor.AddConnector(outputPort.port.PointToScreen(new(5, 5)),
                        pos1,
                        ((SolidColorBrush)outputPort.port.Fill).Color,
                        ((SolidColorBrush)port.Fill).Color,
                        new(_id, _index),
                        new(outputPort.ID, outputPort.Index));
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
