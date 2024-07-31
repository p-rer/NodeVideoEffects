using NodeVideoEffects.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        private bool isDragging;
        private Point clickPosition;
        private Point lastPos;
        public Node(INode node)
        {
            InitializeComponent();
            nodeName.Content = node.Name;
            foreach (Output output in node.Outputs)
            {
                inputsPanel.Children.Add(new OutputPort(output));
            }
            foreach (Input input in node.Inputs)
            {
                inputsPanel.Children.Add(new InputPort(input));
            }
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Node node)
            {
                isDragging = true;
                lastPos = new(e.GetPosition(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(this) as Canvas) as Canvas).X, e.GetPosition(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(this) as Canvas) as Canvas).Y);
                node.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && sender is Node node)
            {
                Point p = new(e.GetPosition(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(this) as Canvas) as Canvas).X, e.GetPosition(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(this) as Canvas) as Canvas).Y);
                Canvas.SetLeft(node, Canvas.GetLeft(node) + p.X - lastPos.X);
                Canvas.SetTop(node, Canvas.GetTop(node) + p.Y - lastPos.Y);
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
    }
}
