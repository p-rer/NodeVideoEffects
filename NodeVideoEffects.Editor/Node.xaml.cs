using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        private string _name;

        private bool isDragging = false;
        private bool isClicking = false;
        private Canvas wrapperCanvas;
        private Editor editor;
        private Point lastPos;

        public string ID { get; }
        public System.Type Type { get; }
        public List<object> Values { get; private set; } = new();
        public List<Color> InputColors { get; private set; } = new();
        public List<Color> OutputColors { get; private set; } = new();

        public event PropertyChangedEventHandler ValueChanged;

        public Node(INode node)
        {
            InitializeComponent();
            nodeName.Content = _name = node.Name;
            ID = node.Id;
            Type = node.GetType();
            {
                int index = 0;
                foreach (Output output in node.Outputs)
                {
                    outputsPanel.Children.Add(new OutputPort(output, node.Id, index));
                    OutputColors.Add(output.Color);
                    index++;
                }
            }
            {
                int index = 0;
                foreach (Input input in node.Inputs)
                {
                    inputsPanel.Children.Add(new InputPort(input, node.Id, index));
                    Values.Add(input.Value);
                    InputColors.Add(input.Color);
                    input.PropertyChanged += (s, e) => ValueChanged?.Invoke(this, new(_name));
                    index++;
                }
            }
            Loaded += (s, e) =>
            {
                wrapperCanvas = FindParent<Canvas>(this);
                editor = FindParent<Editor>(this);
            };
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        internal Point GetPortPoint(PortType type, int index)
        {
            if (type == PortType.Input)
            {
                return (inputsPanel.Children[index] as InputPort).port.PointToScreen(new(5, 5));
            }
            else
            {
                return (outputsPanel.Children[index] as OutputPort).port.PointToScreen(new(5, 5));
            }
        }

        internal enum PortType
        {
            Input,
            Output
        }

        #region Move node
        public event PropertyChangedEventHandler Moved;
        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Node node)
            {
                isDragging = false;
                isClicking = true;
                lastPos = e.GetPosition(wrapperCanvas);
                editor.RestoreChild(this);
                node.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Node node && isClicking)
            {
                if (!isDragging)
                {
                    if (Math.Abs(lastPos.X - e.GetPosition(wrapperCanvas).X) > SystemParameters.MinimumHorizontalDragDistance
                        || Math.Abs(lastPos.Y - e.GetPosition(wrapperCanvas).Y) > SystemParameters.MinimumHorizontalDragDistance)
                    isDragging = true;
                }
                if (isDragging)
                {
                    Point p = e.GetPosition(wrapperCanvas);
                    Move(p.X - lastPos.X, p.Y - lastPos.Y);
                    editor.MoveNode(this, p.X - lastPos.X, p.Y - lastPos.Y);
                    lastPos = p;
                    e.Handled = true;
                }
            }
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isClicking && sender is Node node)
            {
                if (!isDragging)
                    editor.ToggleSelection(this, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
                isClicking = false;
                isDragging = false;
                node.ReleaseMouseCapture();
                SubmitMoving();
                editor.SubmitMoving(this);
                e.Handled = true;
            }
        }

        internal void Move(double dx, double dy)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + dx);
            Canvas.SetTop(this, Canvas.GetTop(this) + dy);
            editor.MoveConnector(ID, new(dx, dy));
        }

        internal void SubmitMoving()
        {
            Moved?.Invoke(this, new PropertyChangedEventArgs(_name));
        }
        #endregion
    }
}
