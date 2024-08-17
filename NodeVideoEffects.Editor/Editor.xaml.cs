using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Editor : UserControl
    {
        private ScaleTransform scaleTransform;
        private TranslateTransform translateTransform;

        private Point lastPos;
        private bool isDragging;
        double scale;
        private Rect wrapRect;

        private Path? previewPath;

        Dictionary<(string, string), Connector> connectors = new();
        Dictionary<string, NodeInfo> infos = new();
        Dictionary<string, Node> nodes = new();
        bool isFirst = true;

        public List<NodeInfo> Nodes { get 
            {
                return infos.Values.ToList();
            }
            set 
            {
                value.ForEach(value =>
                {
                    infos.Add(value.ID, value);
                });
            }
        }

        public Editor()
        {
            InitializeComponent();

            scaleTransform = new ScaleTransform();
            translateTransform = new TranslateTransform();

            scale = scaleTransform.ScaleX;

            canvas.LayoutTransform = scaleTransform;
            canvas.RenderTransform = translateTransform;

            this.MouseWheel += Canvas_MouseWheel;
            this.MouseDown += Canvas_Down;
            this.MouseUp += Canvas_Up;
            this.MouseMove += new MouseEventHandler(Canvas_MouseMove);

            zoomValue.Text = ((int)(scale * 100)).ToString() + "%";

            this.Loaded += EditorLoaded;
        }

        public event PropertyChangedEventHandler NodesUpdated;

        private void OnNodesUpdated()
        {
            NodesUpdated?.Invoke(this, new PropertyChangedEventArgs(nameof(Editor)));
        }

        #region Draw Editor
        private void EditorLoaded(object sender, RoutedEventArgs e)
        {
            UpdateScrollBar();

            canvas.LayoutUpdated += (s, e) => UpdateScrollBar();
            wrapper_canvas.SizeChanged += (s, e) => UpdateScrollBar();

            BuildNodes();
        }

        private void BuildNodes()
        {
            if(isFirst){
                foreach(NodeInfo info in Nodes)
                {
                    INode node = NodesManager.GetNode(info.ID);
                    AddChildren(new(node), info.X, info.Y);
                }
                foreach (NodeInfo info in Nodes)
                {
                    nodes[info.ID].Loaded += (s, e) =>
                    {
                        for (int i = 0; i < info.Connections.Count; i++)
                        {
                            if (info.Connections[i].id != "")
                            {
                                Point inputPoint = nodes[info.ID].GetPortPoint(Node.PortType.Input, i);
                                Point outputPoint = nodes[info.Connections[i].id].GetPortPoint(Node.PortType.Output, info.Connections[i].index);
                                INode node = NodesManager.GetNode(info.ID);
                                Color inputColor = node.Inputs[i].Color;
                                Color outputColor = NodesManager.GetNode(node.Inputs[i].Connection.Value.id).Outputs[node.Inputs[i].Connection.Value.index].Color;
                                AddConnector(outputPoint, inputPoint,
                                    inputColor, outputColor,
                                    new(info.ID, i), new(info.Connections[i].id, info.Connections[i].index));
                            }
                        }
                    };
                }
                isFirst = false;
            }
        }

        private Point ConvertToTransform(Point p)
        {
            return new((p.X - translateTransform.X) / scale, (p.Y - translateTransform.Y) / scale);
        }

        public void AddChildren(Node node, double x, double y)
        {
            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
            canvas.Children.Add(node);
            Input[] inputs = NodesManager.GetNode(node.ID).Inputs??[];
            List<Connection> connections = new();
            for(int i = 0; i < inputs.Length; i++)
            {
                connections.Add(inputs[i].Connection??new());
            }
            if (!infos.ContainsKey(node.ID))
                infos.Add(node.ID, new(node.ID, node.Type, node.Values, x, y, connections));
            nodes.Add(node.ID, node);
            node.Moved += (s, e) =>
            {
                infos[node.ID].X = Canvas.GetLeft(node);
                infos[node.ID].Y = Canvas.GetTop(node);
                OnNodesUpdated();
            };
            node.ValueChanged += (s, e) =>
            {
                try
                {
                    List<object?> value = new();
                    foreach (object input in node.inputsPanel.Children)
                    {
                        value.Add((input as InputPort)?.Value);
                    }
                    infos[node.ID].Values = value;
                    OnNodesUpdated();
                }
                catch { }
            };
        }

        public void PreviewConnection(Point pos1, Point pos2)
        {
            if (previewPath != null) canvas.Children.Remove(previewPath);
            canvas.Children.Add(previewPath = new Path()
            {
                Data = new LineGeometry()
                {
                    StartPoint = ConvertToTransform(pos1),
                    EndPoint = ConvertToTransform(pos2)
                },
                Stroke = SystemColors.GrayTextBrush,
                StrokeThickness = 1,
                StrokeDashArray = new([5.0, 5.0]),
                IsHitTestVisible = false
            });
        }

        public void RemovePreviewConnection()
        {
            if (previewPath != null) canvas.Children.Remove(previewPath);
            previewPath = null;
        }

        public void AddConnector(Point pos1, Point pos2, Color col1, Color col2, Connection inputPort, Connection outputPort)
        {
            if (!connectors.ContainsKey((inputPort.id + inputPort.index, outputPort.id + outputPort.index)))
            {
                Connector connector;
                canvas.Children.Add(connector = new Connector()
                {
                    StartPoint = ConvertToTransform(PointFromScreen(pos1)),
                    EndPoint = ConvertToTransform(PointFromScreen(pos2)),
                    StartColor = col1,
                    EndColor = col2,
                    IsHitTestVisible = false
                });
                connector.SetValue(Panel.ZIndexProperty, -1);
                connectors.Add((inputPort.id + inputPort.index, outputPort.id + outputPort.index), connector);
                infos[inputPort.id].Connections[inputPort.index] = outputPort;
                OnNodesUpdated();
            }
        }

        public void RemoveConnector(string id, int index)
        {
            try
            {
                Connector connector = connectors
                    .Where(kvp => kvp.Key.Item1 == id + index)
                    .Select(kvp => kvp.Value)
                    .ToList()[0];
                canvas.Children.Remove(connector);
                connectors.Remove(connectors
                    .Where(kvp => kvp.Value == connector)
                    .Select(kvp => kvp.Key)
                    .ToList()[0]);
                infos[id].Connections[index] = new();
                OnNodesUpdated();
            }
            catch { }
        }

        public void MoveConnector(string id, Point d)
        {
            try
            {
                foreach(Connector connector in connectors
                    .Where(kvp => kvp.Key.Item1.StartsWith(id))
                    .Select(kvp => kvp.Value)
                    .ToList())
                {
                    connector.EndPoint = new(connector.EndPoint.X + d.X, connector.EndPoint.Y + d.Y);
                }
                foreach (Connector connector in connectors
                    .Where(kvp => kvp.Key.Item2.StartsWith(id))
                    .Select(kvp => kvp.Value)
                    .ToList())
                {
                    connector.StartPoint = new(connector.StartPoint.X + d.X, connector.StartPoint.Y + d.Y);
                }
            }
            catch { }
        }

        public void UpdateScrollBar()
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (UIElement element in canvas.Children)
            {
                if (element is FrameworkElement)
                {
                    var fe = (FrameworkElement)element;

                    double left = Canvas.GetLeft(fe) * scale + translateTransform.X;
                    double top = Canvas.GetTop(fe) * scale + translateTransform.Y;
                    double right = left + fe.ActualWidth * scale;
                    double bottom = top + fe.ActualHeight * scale;

                    if (left < minX) minX = left;
                    if (top < minY) minY = top;
                    if (right > maxX) maxX = right;
                    if (bottom > maxY) maxY = bottom;
                }
            }

            if (minX == double.MaxValue || minY == double.MaxValue || maxX == double.MinValue || maxY == double.MinValue)
            {
                wrapRect = Rect.Empty;
            }
            else
            {
                wrapRect = new Rect(
                    new Point(
                        minX < 0 ? minX : 0,
                        minY < 0 ? minY : 0),
                    new Point(
                        maxX > wrapper_canvas.ActualWidth ? maxX : wrapper_canvas.ActualWidth,
                        maxY > wrapper_canvas.ActualHeight ? maxY : wrapper_canvas.ActualHeight
                    )
                );
            }

            horizontalScrollBar.Width = (wrapper_canvas.ActualWidth * wrapper_canvas.ActualWidth) / wrapRect.Width;
            Canvas.SetLeft(horizontalScrollBar, -wrapRect.Left * wrapper_canvas.ActualWidth / wrapRect.Width);

            verticalScrollBar.Height = (wrapper_canvas.ActualHeight * wrapper_canvas.ActualHeight) / wrapRect.Height;
            Canvas.SetTop(verticalScrollBar, -wrapRect.Top * wrapper_canvas.ActualHeight / wrapRect.Height);
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double rate = e.Delta > 0 ? 1.1 : 1 / 1.1;
            double zoom = scale * rate;
            Canvas_Zoom(zoom, e);
        }

        private void Canvas_Down(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                lastPos = new(e.GetPosition(this).X, e.GetPosition(this).Y);
                isDragging = true;
                this.CaptureMouse();
            }
        }

        private void Canvas_Up(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
            {
                isDragging = false;
                this.ReleaseMouseCapture();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = new(e.GetPosition(this).X, e.GetPosition(this).Y);
                translateTransform.X += p.X - lastPos.X;
                translateTransform.Y += p.Y - lastPos.Y;
                lastPos = p;

                UpdateScrollBar();
            }
        }

        private void Canvas_Zoom(double zoom)
        {
            double rate = zoom / scale;
            if (zoom > 4) rate = 4 / scale;
            if (zoom < 0.1) rate = 0.1 / scale;
            scale *= rate;
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
            translateTransform.X += (rate - 1) * (translateTransform.X - wrapper_canvas.ActualWidth / 2);
            translateTransform.Y += (rate - 1) * (translateTransform.Y - wrapper_canvas.ActualHeight / 2);
            zoomValue.Text = ((int)(scale * 100)).ToString() + "%";
        }

        private void Canvas_Zoom(double zoom, MouseWheelEventArgs e)
        {
            double rate = zoom / scale;
            if (zoom > 4) rate = 4 / scale;
            if (zoom < 0.1) rate = 0.1 / scale;
            scale *= rate;
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
            translateTransform.X += (rate - 1) * (translateTransform.X - e.GetPosition(wrapper_canvas).X);
            translateTransform.Y += (rate - 1) * (translateTransform.Y - e.GetPosition(wrapper_canvas).Y);
            zoomValue.Text = ((int)(scale * 100)).ToString() + "%";
        }
        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (new Regex("[^0-9]").IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void zoomValueSubmit(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (zoomValue.Text == "") zoomValue.Text = "100";
                if (zoomValue.Text.EndsWith("%")) zoomValue.Text = zoomValue.Text.Remove(zoomValue.Text.Length - 1);
                double zoom;
                try
                {
                    zoom = double.Parse(zoomValue.Text) / 100;
                }
                catch (Exception)
                {
                    zoom = 1;
                }
                Canvas_Zoom(zoom);
                Keyboard.ClearFocus();
            }
        }
        #endregion
    }
}
