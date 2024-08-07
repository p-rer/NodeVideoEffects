using NodeVideoEffects.Type;
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
        private string _id;

        private bool isDragging;
        private Canvas wrapperCanvas;
        private Editor editor;
        private Point lastPos;
        public Node(INode node)
        {
            InitializeComponent();
            nodeName.Content = _name = node.Name;
            _id = node.Id;
            {
                int index = 0;
                foreach (Output output in node.Outputs)
                {
                    outputsPanel.Children.Add(new OutputPort(output, node.Id, index));
                    index++;
                }
            }
            {
                int index = 0;
                foreach (Input input in node.Inputs)
                {
                    inputsPanel.Children.Add(new InputPort(input, node.Id, index));
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

        #region Move node
        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Node node)
            {
                isDragging = true;
                lastPos = new(e.GetPosition(wrapperCanvas).X, e.GetPosition(wrapperCanvas).Y);
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
                editor.MoveConnector(_id, new(p.X - lastPos.X, p.Y - lastPos.Y));
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
                e.Handled = true;
            }
        }
        #endregion
    }
}
