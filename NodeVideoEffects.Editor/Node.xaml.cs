using NodeVideoEffects.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Input[]? _inputs;
        private Output[]? _outputs;
        private List<InputPort> _inputPorts = new();
        private List<OutputPort> _outputPorts = new();

        private string _name;
        private string _id;

        private bool isDragging;
        private Canvas wrapperCanvas;
        private Point lastPos;
        public Node(INode node)
        {
            InitializeComponent();
            nodeName.Content = node.Name;
            _inputs = node.Inputs;
            _outputs = node.Outputs;
            foreach (Output output in _outputs)
            {
                OutputPort outputPort = new(output);
                _outputPorts.Add(outputPort);
                outputsPanel.Children.Add(outputPort);
            }

            foreach (Input input in _inputs)
            {
                InputPort inputPort = new (input);
                inputPort.PropertyChanged += Input_PropertyChanged;
                _inputPorts.Add(inputPort);
                inputsPanel.Children.Add(inputPort);
            }
            Loaded += (s, e) =>
            {
                wrapperCanvas = VisualTreeHelper.GetParent(this) as Canvas;
            };
        }

        private void Input_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Input.Value))
            {
                foreach (Output output in _outputs)
                {
                    output.IsSuccess = false;
                }
            }
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
