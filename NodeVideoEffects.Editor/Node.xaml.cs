using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        private string _name;

        private bool isDragging;
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
            categorySign.Fill = new SolidColorBrush(node.Color);
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
                isDragging = true;
                lastPos = new(e.GetPosition(wrapperCanvas).X, e.GetPosition(wrapperCanvas).Y);
                int maxZ = editor.canvas.Children.OfType<UIElement>().Max(Panel.GetZIndex);
                if (Panel.GetZIndex(this) <= maxZ)
                    SetValue(Panel.ZIndexProperty, maxZ + 1);
                node.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && sender is Node node)
            {
                Point p = new(e.GetPosition(wrapperCanvas).X, e.GetPosition(wrapperCanvas).Y);
                Canvas.SetLeft(node, Canvas.GetLeft(node) + p.X - lastPos.X);
                Canvas.SetTop(node, Canvas.GetTop(node) + p.Y - lastPos.Y);
                editor.MoveConnector(ID, new(p.X - lastPos.X, p.Y - lastPos.Y));
                lastPos = p;
                e.Handled = true;
            }
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && sender is Node node)
            {
                isDragging = false;
                node.ReleaseMouseCapture();
                Moved?.Invoke(this, new PropertyChangedEventArgs(_name));
                e.Handled = true;
            }
        }
        #endregion
    }
}
