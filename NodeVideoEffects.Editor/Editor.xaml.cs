using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        private Point nowPos;
        private bool isDragging;
        private bool isSelecting;
        double scale;
        private Rect wrapRect;
        private Rectangle selectingRect = new();

        private Path? previewPath;

        Dictionary<(string, string), Connector> connectors = new();
        Dictionary<string, NodeInfo> infos = new();
        Dictionary<string, Node> nodes = new();

        List<Node> selectingNodes = new();

        public List<NodeInfo> Nodes
        {
            get
            {
                return infos.Values.ToList();
            }
            set
            {
                infos.Clear();
                value.ForEach(value =>
                {
                    infos.Add(value.ID, value);
                });
            }
        }

        public string ItemID { get; set; }

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

        public void OnNodesUpdated()
        {
            NodesUpdated?.Invoke(this, new PropertyChangedEventArgs(nameof(Editor)));
        }

        #region Draw Editor
        private void DrawDotPattern(Canvas canvas, double dotSpacingX, double dotSpacingY, double dotSize, double scale, Point offset)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                for (double x = 0; x < dotSpacingX * 10; x += dotSpacingX)
                {
                    for (double y = 0; y < dotSpacingY * 10; y += dotSpacingY)
                    {
                        dc.DrawEllipse(SystemColors.GrayTextBrush, null, new Point(x, y), dotSize, dotSize);
                    }
                }
            }

            VisualBrush dotBrush = new VisualBrush(drawingVisual)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, dotSpacingX, dotSpacingY),
                ViewportUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None
            };

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scale, scale));
            transformGroup.Children.Add(new TranslateTransform(offset.X, offset.Y));
            dotBrush.Transform = transformGroup;

            canvas.Background = dotBrush;
        }

        private void EditorLoaded(object sender, RoutedEventArgs e)
        {
            UpdateScrollBar();
            DrawDotPattern(wrapper_canvas, 50, 50, 1, scale, new Point(translateTransform.X, translateTransform.Y));

            canvas.LayoutUpdated += (s, e) =>
            {
                UpdateScrollBar();
                DrawDotPattern(wrapper_canvas, 50, 50, 1, scale, new Point(translateTransform.X, translateTransform.Y));
            };
            wrapper_canvas.SizeChanged += (s, e) =>
            {
                UpdateScrollBar();
                DrawDotPattern(wrapper_canvas, 50, 50, 1, scale, new Point(translateTransform.X, translateTransform.Y));
            };

            BuildNodes();
        }

#if DEBUG
        private void LogConnection(string message = "")
        {
            Console.WriteLine($"[UTC {DateTime.UtcNow.ToString("HH:mm:ss.fff")}] {message}");
            foreach (NodeInfo info in Nodes)
            {
                for (int i = 0; i < info.Connections.Count; i++)
                {
                    Console.WriteLine($"{info.ID}({info.Type}):\n[\"{info.Connections[i].id}\", {i}]\n");
                }
            }
            Console.WriteLine("\n");
        }
#endif

        private void BuildNodes()
        {
            foreach (NodeInfo info in Nodes)
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
#if DEBUG
                        LogConnection("Called by BuildNodes function");
#endif
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
                            (nodes[info.ID].inputsPanel.Children[i] as InputPort).portControl.Visibility = Visibility.Hidden;
                        }
                    }
                };
            }
        }

        public async void RebuildNodes(List<NodeInfo> infos)
        {
            // Remove deleted nodes
            infos
            .Where(info => !nodes.ContainsKey(info.ID))
            .Select(info =>
            {
                return Task.Run(() => NodesManager.RemoveNode(info.ID));
            });

            // Create new node
            infos.ForEach(info =>
            {
                if (!nodes.ContainsKey(info.ID))
                {
                    System.Type? type = System.Type.GetType(info.Type);
                    if (type != null)
                    {
                        INode? obj = Activator.CreateInstance(type) as INode;
                        if (obj != null)
                        {
                            obj.Id = info.ID;
                            NodesManager.AddNode(info.ID, obj);
                            for (int i = 0; i < info.Values.Count; i++)
                            {
                                obj.SetInput(i, info.Values[i]);
                                obj.SetInputConnection(i, info.Connections[i]);
                            }
                        }
                    }
                }
            });

#if DEBUG
            LogConnection("Before calling RebuildNodes Function");
#endif

            await Task.WhenAll(infos.Select(info =>
            {
                return Task.Run(() =>
                {
                    if (this.infos.ContainsKey(info.ID))
                    {
                        int i = 0;
                        foreach (Connection connection in this.infos[info.ID].Connections)
                        {
                            if (connection.id != info.Connections[i].id)
                            {
                                if (info.Connections[i].id == "")
                                    NodesManager.GetNode(info.ID)?.RemoveInputConnection(i);
                                else
                                    NodesManager.GetNode(info.ID)?.SetInputConnection(i, info.Connections[i]);
                            }
                            i++;
                        }
                    }
                });
            }));

            Nodes = infos;

#if DEBUG
            LogConnection("After calling RebuildNodes Function");
#endif

            Dispatcher.Invoke(() =>
            {
                canvas.Children.Clear();
                nodes.Clear();
                selectingNodes.Clear();
                connectors.Clear();

                BuildNodes();
            });
        }

        internal Point ConvertToTransform(Point p)
        {
            return new((p.X - translateTransform.X) / scale, (p.Y - translateTransform.Y) / scale);
        }

        public void AddChildren(Node node, double x, double y)
        {
            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
            canvas.Children.Add(node);
            Input[] inputs = NodesManager.GetNode(node.ID)?.Inputs ?? [];
            List<Connection> connections = new();
            for (int i = 0; i < inputs.Length; i++)
            {
                connections.Add(inputs[i].Connection ?? new());
            }
            if (!infos.TryAdd(node.ID, new(node.ID, node.Type, node.Values, x, y, connections)))
                infos[node.ID] = new(node.ID, node.Type, node.Values, x, y, connections);
            if (!nodes.TryAdd(node.ID, node))
                nodes[node.ID] = node;
            node.Moved += (s, e) =>
            {
                infos[node.ID].X = Canvas.GetLeft(node);
                infos[node.ID].Y = Canvas.GetTop(node);
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

        public void RemoveChildren()
        {
            if (selectingNodes.Count > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    foreach (Node node in selectingNodes)
                    {
                        if (node.Type != typeof(InputNode) && node.Type != typeof(OutputNode))
                        {
                            canvas.Children.Remove(node);
                            foreach (OutputPort output in node.outputsPanel.Children)
                            {
                                output.RemoveAllConnection();
                            }
                            foreach (InputPort input in node.inputsPanel.Children)
                            {
                                input.RemoveConnection();
                            }
                            infos.Remove(node.ID);
                            nodes.Remove(node.ID);
                            NodesManager.RemoveNode(node.ID);
                        }
                    }
                    selectingNodes.Clear();
                });
            }
            OnNodesUpdated();
        }

        public void RestoreChild(Node node)
        {
            canvas.Children.Remove(node);
            canvas.Children.Add(node);
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
            if (connectors.ContainsKey((inputPort.id + ";" + inputPort.index, outputPort.id + ";" + outputPort.index)))
            {
                canvas.Children.Remove(connectors[(inputPort.id + ";" + inputPort.index, outputPort.id + ";" + outputPort.index)]);
                connectors.Remove((inputPort.id + ";" + inputPort.index, outputPort.id + ";" + outputPort.index));
            }
            if ((connectors.Where(kvp => kvp.Key.Item1 == inputPort.id + ";" + inputPort.index).ToList().Count) > 0)
            {
                NodesManager.GetNode(infos[inputPort.id].Connections[inputPort.index].id)
                    ?.Outputs[infos[inputPort.id].Connections[inputPort.index].index]
                    .RemoveConnection(inputPort.id, inputPort.index);
                RemoveInputConnector(inputPort.id, inputPort.index);
                NodesManager.GetNode(inputPort.id)?.SetInputConnection(inputPort.index, outputPort);
            }
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
            connectors.Add((inputPort.id + ";" + inputPort.index, outputPort.id + ";" + outputPort.index), connector);
            infos[inputPort.id].Connections[inputPort.index] = outputPort;

#if DEBUG
            LogConnection($"Called by AddConnector function({inputPort.id}, {inputPort.index})");
#endif
        }

        public void RemoveInputConnector(string id, int index)
        {
            try
            {
                Connector connector = connectors
                    .Where(kvp => kvp.Key.Item1 == id + ";" + index)
                    .Select(kvp => kvp.Value)
                    .ToList()[0];
                canvas.Children.Remove(connector);
                connectors.Remove(connectors
                    .Where(kvp => kvp.Value == connector)
                    .Select(kvp => kvp.Key)
                    .ToList()[0]);
                infos[id].Connections[index] = new();
#if DEBUG
                LogConnection($"Called by RemoveInputConnector function({id}, {index})");
#endif
            }
            catch { }
        }

        public void RemoveOutputConnector(string id, int index)
        {
            try
            {
                List<Connector> connectors = this.connectors
                    .Where(kvp => kvp.Key.Item2 == id + ";" + index)
                    .Select(kvp => kvp.Value)
                    .ToList();
                foreach (Connector connector in connectors)
                {
                    (string, string) key = this.connectors
                        .Where(kvp => kvp.Value == connector)
                        .Select(kvp => kvp.Key)
                        .ToList()[0];
                    int sepIndex = key.Item1.IndexOf(";");
                    (nodes[key.Item1[0..sepIndex]].inputsPanel.Children[int.Parse(key.Item1[(sepIndex + 1)..^0])] as InputPort)?.RemoveConnection();
                    infos[key.Item1[0..sepIndex]].Connections[int.Parse(key.Item1[(sepIndex + 1)..^0])] = new();
                    this.connectors.Remove(key);
#if DEBUG
                    LogConnection($"Called by RemoveOutputConnector function({key.Item1[0..sepIndex]}, {key.Item1[(sepIndex + 1)..^0]})");
#endif
                }
            }
            catch { }
        }

        public void MoveConnector(string id, Point d)
        {
            try
            {
                foreach (Connector connector in connectors
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
                lastPos = e.GetPosition(this);
                isDragging = true;
                this.CaptureMouse();
            }
            else if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                lastPos = e.GetPosition(this);
                isSelecting = true;
                selectingRect = CreateSelectingRectangleFromPoints(ConvertToTransform(lastPos), ConvertToTransform(lastPos));
                canvas.Children.Add(selectingRect);
                Task.Run(() =>
                {
                    while (isSelecting)
                    {
                        int f = GetDirectionIfOutside(nowPos);
                        this.Dispatcher.Invoke(() =>
                        {
                            const int step = 15;
                            if ((f & 1) != 0)
                            {
                                translateTransform.Y += step;
                                lastPos.Y += step;
                            }
                            if ((f & 2) != 0)
                            {
                                translateTransform.X -= step;
                                lastPos.X -= step;
                            }
                            if ((f & 4) != 0)
                            {
                                translateTransform.Y -= step;
                                lastPos.Y -= step;
                            }
                            if ((f & 8) != 0)
                            {
                                translateTransform.X += step;
                                lastPos.X += step;
                            }
                            canvas.Children.Remove(selectingRect);
                            selectingRect = CreateSelectingRectangleFromPoints(ConvertToTransform(lastPos), ConvertToTransform(nowPos));
                            canvas.Children.Add(selectingRect);
                        });
                        Task.Delay(50).Wait();
                    }
                });
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
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Released)
            {
                isSelecting = false;
                canvas.Children.Remove(selectingRect);
                foreach (Node node in selectingNodes)
                {
                    Task.Run(() => Dispatcher.Invoke(() =>
                    {
                        node.Width -= 8;
                        node.Height -= 8;
                        Canvas.SetLeft(node, Canvas.GetLeft(node) + 4);
                        Canvas.SetTop(node, Canvas.GetTop(node) + 4);
                        node.Padding = new(0);
                        node.BorderThickness = new(0);
                        node.BorderBrush = null;
                    }));
                }
                selectingNodes = GetElementsInsideRect(canvas,
                                                       new(Canvas.GetLeft(selectingRect),
                                                                    Canvas.GetTop(selectingRect),
                                                                    selectingRect.ActualWidth,
                                                                    selectingRect.ActualHeight));
                selectingRect = new();
                foreach (Node node in selectingNodes)
                {
                    Task.Run(() => Dispatcher.Invoke(() =>
                    {
                        node.Width += 8;
                        node.Height += 8;
                        Canvas.SetLeft(node, Canvas.GetLeft(node) - 4);
                        Canvas.SetTop(node, Canvas.GetTop(node) - 4);
                        node.Padding = new(2.0);
                        node.BorderThickness = new(2.0);
                        node.BorderBrush = SystemColors.HighlightBrush;
                    }));
                }
                this.ReleaseMouseCapture();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            nowPos = e.GetPosition(this);
            if (isDragging)
            {
                translateTransform.X += nowPos.X - lastPos.X;
                translateTransform.Y += nowPos.Y - lastPos.Y;
                lastPos = nowPos;

                UpdateScrollBar();
            }
            else if (isSelecting)
            {
                canvas.Children.Remove(selectingRect);
                selectingRect = CreateSelectingRectangleFromPoints(ConvertToTransform(lastPos), ConvertToTransform(nowPos));
                canvas.Children.Add(selectingRect);
            }
        }

        public void AllSelect()
        {
            foreach (Node node in selectingNodes)
            {
                Task.Run(() => Dispatcher.Invoke(() =>
                {
                    node.Width -= 8;
                    node.Height -= 8;
                    Canvas.SetLeft(node, Canvas.GetLeft(node) + 4);
                    Canvas.SetTop(node, Canvas.GetTop(node) + 4);
                    node.Padding = new(0);
                    node.BorderThickness = new(0);
                    node.BorderBrush = null;
                }));
            }
            selectingNodes = nodes.Select((kvp) => kvp.Value).ToList();
            foreach (Node node in selectingNodes)
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        node.Width += 8;
                        node.Height += 8;
                        Canvas.SetLeft(node, Canvas.GetLeft(node) - 4);
                        Canvas.SetTop(node, Canvas.GetTop(node) - 4);
                        node.Padding = new(2.0);
                        node.BorderThickness = new(2.0);
                        node.BorderBrush = SystemColors.HighlightBrush;
                    });
                });
            }
        }

        internal void ToggleSelection(Node selectNode, bool multiple)
        {
            Task.Run(() =>
            {
                if (!multiple)
                {
                    foreach (Node node in selectingNodes)
                    {
                        Task.Run(() =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                node.Width -= 8;
                                node.Height -= 8;
                                Canvas.SetLeft(node, Canvas.GetLeft(node) + 4);
                                Canvas.SetTop(node, Canvas.GetTop(node) + 4);
                                node.Padding = new(0);
                                node.BorderThickness = new(0);
                                node.BorderBrush = null;
                            });
                        });
                    }
                    selectingNodes = [selectNode];
                }
                else
                {
                    if (selectingNodes.Contains(selectNode))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            selectingNodes.Remove(selectNode);
                            selectNode.Width -= 8;
                            selectNode.Height -= 8;
                            Canvas.SetLeft(selectNode, Canvas.GetLeft(selectNode) + 4);
                            Canvas.SetTop(selectNode, Canvas.GetTop(selectNode) + 4);
                            selectNode.Padding = new(0);
                            selectNode.BorderThickness = new(0);
                            selectNode.BorderBrush = null;
                        });
                        return;
                    }
                    else
                        selectingNodes.Add(selectNode);
                }
                Dispatcher.Invoke(() =>
                {
                    selectNode.Width += 8;
                    selectNode.Height += 8;
                    Canvas.SetLeft(selectNode, Canvas.GetLeft(selectNode) - 4);
                    Canvas.SetTop(selectNode, Canvas.GetTop(selectNode) - 4);
                    selectNode.Padding = new(2.0);
                    selectNode.BorderThickness = new(2.0);
                    selectNode.BorderBrush = SystemColors.HighlightBrush;
                });
            });
        }

        private Rectangle CreateSelectingRectangleFromPoints(Point p1, Point p2)
        {
            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p2.X - p1.X);
            double height = Math.Abs(p2.Y - p1.Y);

            Rectangle rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = null,
                Stroke = SystemColors.GrayTextBrush,
                StrokeThickness = 1,
                StrokeDashArray = new([5.0, 5.0]),
                IsHitTestVisible = false
            };

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            return rectangle;
        }

        internal void MoveNode(Node node, double dx, double dy)
        {
            if (!selectingNodes.Contains(node))
                ToggleSelection(node, false);
            else
            {
                foreach (Node selectedNode in selectingNodes)
                {
                    Task.Run(() => Dispatcher.Invoke(() => selectedNode.Move(dx, dy)));
                }
            }
        }

        internal void SubmitMoving(Node node)
        {
            foreach (Node selectedNode in selectingNodes)
            {
                selectedNode.SubmitMoving();
            }
            OnNodesUpdated();
        }

        public List<Node> GetElementsInsideRect(Canvas canvas, Rect rect)
        {
            var elementsInsideRect = new List<Node>();

            foreach (UIElement element in canvas.Children)
            {
                if (element is not Node)
                    continue;
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);

                double width = element.RenderSize.Width;
                double height = element.RenderSize.Height;

                Rect elementRect = new Rect(new Point(left, top), new Size(width, height));

                if (rect.Contains(elementRect))
                {
                    elementsInsideRect.Add((Node)element);
                }
            }

            return elementsInsideRect;
        }

        public int GetDirectionIfOutside(Point p)
        {
            double left = 0;
            double top = 0;
            double right = ActualWidth;
            double bottom = ActualHeight;
            int n = 0;
            if (p.X < left)
                n += 8;
            if (p.X > right)
                n += 2;
            if (p.Y < top)
                n += 1;
            if (p.Y > bottom)
                n += 4;
            return n;
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
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
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
