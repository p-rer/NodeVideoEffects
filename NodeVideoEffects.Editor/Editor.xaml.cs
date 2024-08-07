using NodeVideoEffects.Type;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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

        #region Draw Editor
        private void EditorLoaded(object sender, RoutedEventArgs e)
        {
            UpdateScrollBar();

            canvas.LayoutUpdated += (s, args) => UpdateScrollBar();
            wrapper_canvas.SizeChanged += (s, args) => UpdateScrollBar();
        }

        private Point ConvertToTransform(Point p)
        {
            return new((p.X - translateTransform.X) / scale, (p.Y - translateTransform.Y) / scale);
        }

        public void AddChildren(Node obj, double x, double y)
        {
            Canvas.SetLeft(obj, x);
            Canvas.SetTop(obj, y);
            canvas.Children.Add((UIElement)obj);
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

        public void AddConnector(Point pos1, Point pos2, Connection inputPort, Connection outputPort)
        {
            Connector connector;
            canvas.Children.Add(connector = new Connector()
            {
                StartPoint = ConvertToTransform(PointFromScreen(pos1)),
                EndPoint = ConvertToTransform(PointFromScreen(pos2)),
                StartPort = inputPort,
                EndPort = outputPort,
                IsHitTestVisible = false
            });
            connectors.Add((inputPort.id + inputPort.index, outputPort.id + outputPort.index), connector);
        }

        public void RemoveConnectorFromInputPort(string id, int index)
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
            horizontalScrollBar.Margin = new Thickness(-wrapRect.Left * wrapper_canvas.ActualWidth / wrapRect.Width, 0, 0, 0);

            verticalScrollBar.Height = (wrapper_canvas.ActualHeight * wrapper_canvas.ActualHeight) / wrapRect.Height;
            verticalScrollBar.Margin = new Thickness(0, -wrapRect.Top * wrapper_canvas.ActualHeight / wrapRect.Height, 0, 0);
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
