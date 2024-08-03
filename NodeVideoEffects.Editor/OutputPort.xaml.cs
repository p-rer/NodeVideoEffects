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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private bool isMouseDown = false;
        public OutputPort(Output output, string id, int index)
        {
            InitializeComponent();
            _output = output;
            _id = id;
            _index = index;
            portName.Content = output.Name;
            ToolTip = new();
            ToolTipOpening += OutputPort_ToolTipOpening;
        }

        private void OutputPort_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            Task<object> @object = NodesManager.GetOutputValue(_id, _index);
            @object.Wait();
            ToolTip = @object.Result;
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

        private void port_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            port.CaptureMouse();
            e.Handled = true;
        }

        private void port_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        private void port_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;

            // Get the control under the mouse pointer
            Point position = e.GetPosition(FindParent<Editor>(this));
            HitTestResult result = VisualTreeHelper.HitTest(FindParent<Editor>(this), position);

            if (result != null)
            {
                var element = result.VisualHit as FrameworkElement;

                if (element != null)
                {
                    SetConnectionToInputPort(element);
                }
            }
            port.ReleaseMouseCapture();
            e.Handled = true;
        }

        private bool SetConnectionToInputPort(DependencyObject element)
        {
            InputPort? inputPort = FindParent<InputPort>(element);
            if (inputPort != null)
            {
                inputPort.SetConnection(_id, _index);
                _output.AddConnection(inputPort.ID, inputPort.Index);
                return true;
            }
            return false;
        }
    }
}
