﻿using NodeVideoEffects.Type;
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

        private void OutputPort_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            Task<object> @object = NodesManager.GetOutputValue(_id, _index);
            @object.Wait();
            ToolTip = @object.Result;
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
                    SetConnectionToInputPort(element, port.PointToScreen(new(5, 5)), position);
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
                inputPort.SetConnection(_id, _index);
                _output.AddConnection(inputPort.ID, inputPort.Index);
                editor.AddConnector(pos1, inputPort.port.PointToScreen(new(5, 5)),
                    ((SolidColorBrush)port.Fill).Color,
                    ((SolidColorBrush)inputPort.port.Fill).Color,
                    new(inputPort.ID, inputPort.Index), new(_id, _index));
                return true;
            }
            return false;
        }
    }
}
