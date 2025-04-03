using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Core;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Editor;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class Editor : INotifyPropertyChanged
{
    private readonly ScaleTransform _scaleTransform;
    private readonly TranslateTransform _translateTransform;

    private Point _lastPos;
    private Point _nowPos;
    private bool _isDragging;
    private bool _isSelecting;
    private double _scale;
    private Rect _wrapRect;
    private Rectangle _selectingRect = new();

    private Path? _previewPath;

    private readonly Dictionary<(string, string), Connector> _connectors = [];
    private readonly Dictionary<string, NodeInfo> _infos = [];
    private readonly Dictionary<string, Node> _nodes = [];

    private List<Node> _selectingNodes = [];
    private int _runningTaskCount;
    private string _infoText = "";

    private static readonly Lock Locker = new();

    private const double Tolerance = 0.001;

    public List<NodeInfo> Nodes
    {
        get => _infos.Values.ToList();
        set
        {
            _infos.Clear();
            value.ForEach(value => { _infos.Add(value.Id, value); });
        }
    }

    public string ItemId { get; set; } = "";

    public Editor()
    {
        InitializeComponent();
        DataContext = this;

        _scaleTransform = new ScaleTransform();
        _translateTransform = new TranslateTransform();

        _scale = _scaleTransform.ScaleX;

        Canvas.LayoutTransform = _scaleTransform;
        Canvas.RenderTransform = _translateTransform;

        MouseWheel += Canvas_MouseWheel;
        MouseDown += Canvas_Down;
        MouseUp += Canvas_Up;
        MouseMove += Canvas_MouseMove;

        ZoomValue.Text = (int)(_scale * 100) + "%";
        InfoText = "Initializing...";

        Loaded += EditorLoaded;
        TaskTracker.TaskCountChanged += OnTaskCountChanged;
    }

    #region StatusBar

    private void OnTaskCountChanged(object? sender, int newCount)
    {
        lock (Locker)
        {
            Dispatcher.Invoke(() => { RunningTaskCount = newCount; });
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private int RunningTaskCount
    {
        set
        {
            if (_runningTaskCount == value) return;
            _runningTaskCount = value;
            OnPropertyChanged(nameof(RunningTaskText));
        }
    }

    public string RunningTaskText =>
        _runningTaskCount == 0 ? "All tasks have completed" : $"Running {_runningTaskCount} task(s)";

    public string InfoText
    {
        get => _infoText;
        set
        {
            if (_infoText == value) return;
            _infoText = value;
            OnPropertyChanged(nameof(InfoText));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    public event PropertyChangedEventHandler? NodesUpdated;

    public void OnNodesUpdated()
    {
        NodesUpdated?.Invoke(this, new PropertyChangedEventArgs(nameof(Editor)));
    }

    private void Editor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Editor) return;
        e.Handled = true;
    }

    #region Draw Editor

    private static void DrawDotPattern(Canvas canvas, double dotSpacingX, double dotSpacingY, double dotSize,
        double scale, Point offset)
    {
        var drawingVisual = new DrawingVisual();
        using var dc = drawingVisual.RenderOpen();
        for (double x = 0; x < dotSpacingX * 10; x += dotSpacingX)
        for (double y = 0; y < dotSpacingY * 10; y += dotSpacingY)
            dc.DrawEllipse(SystemColors.GrayTextBrush, null, new Point(x, y), dotSize, dotSize);

        var dotBrush = new VisualBrush(drawingVisual)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, dotSpacingX, dotSpacingY),
            ViewportUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None
        };

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new ScaleTransform(scale, scale));
        transformGroup.Children.Add(new TranslateTransform(offset.X, offset.Y));
        dotBrush.Transform = transformGroup;

        canvas.Background = dotBrush;
    }

    private async void EditorLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await Dispatcher.InvokeAsync(() =>
            {
                UpdateScrollBar();
                DrawDotPattern(WrapperCanvas, 50, 50, 1, _scale,
                    new Point(_translateTransform.X, _translateTransform.Y));

                Canvas.LayoutUpdated += (_, _) =>
                {
                    UpdateScrollBar();
                    DrawDotPattern(WrapperCanvas, 50, 50, 1, _scale,
                        new Point(_translateTransform.X, _translateTransform.Y));
                };
                WrapperCanvas.SizeChanged += (_, _) =>
                {
                    UpdateScrollBar();
                    DrawDotPattern(WrapperCanvas, 50, 50, 1, _scale,
                        new Point(_translateTransform.X, _translateTransform.Y));
                };

                BuildNodes();
            });
        }
        catch (Exception exception)
        {
            InfoText = "Error occurred while initializing editor";
            Logger.Write(LogLevel.Error, exception.Message, exception);
        }
    }

    private void BuildNodes()
    {
#if DEBUG
        Logger.Write(LogLevel.Debug, $"BuildNodes started.\nNodesCount={Nodes.Count}");
#endif
        try
        {
#if DEBUG
            Logger.Write(LogLevel.Debug, "Starting to add children for each node in Nodes.", Nodes);
#endif
            foreach (var info in Nodes)
            {
#if DEBUG
                Logger.Write(LogLevel.Debug, $"Processing node with ID: {info.Id}.", info);
#endif
                var node = NodesManager.GetNode(info.Id);
#if DEBUG
                Logger.Write(LogLevel.Debug, $"Retrieved node from NodesManager for ID: {info.Id}.", node);
#endif
                AddChildren(new Node(node!), info.X, info.Y);
#if DEBUG
                Logger.Write(LogLevel.Debug, $"Added children for node ID: {info.Id}.", new { info.X, info.Y });
#endif
            }

#if DEBUG
            Logger.Write(LogLevel.Debug, "Starting to set up connections for each node.", Nodes);
#endif
            foreach (var info in Nodes)
            {
#if DEBUG
                Logger.Write(LogLevel.Debug, $"Setting up connections for node ID: {info.Id}.", info);
#endif
                _nodes[info.Id].Loaded += (_, _) =>
                {
#if DEBUG
                    Logger.Write(LogLevel.Debug, $"Node loaded event triggered for ID: {info.Id}.", info);
#endif
                    for (var i = 0; i < info.Connections.Count; i++)
                    {
                        if (info.Connections[i].Id == "") continue;
#if DEBUG
                        Logger.Write(LogLevel.Debug, $"Processing connection {i} for node ID: {info.Id}.",
                            info.Connections[i]);
#endif
                        var inputPoint = _nodes[info.Id].GetPortPoint(Node.PortType.Input, i);
#if DEBUG
                        Logger.Write(LogLevel.Debug,
                            $"Retrieved input point for node ID: {info.Id}, port index: {i}.", inputPoint);
#endif
                        var outputPoint = _nodes[info.Connections[i].Id]
                            .GetPortPoint(Node.PortType.Output, info.Connections[i].Index);
#if DEBUG
                        Logger.Write(LogLevel.Debug,
                            $"Retrieved output point for connection ID: {info.Connections[i].Id}, port index: {info.Connections[i].Index}.",
                            outputPoint);
#endif
                        var node = NodesManager.GetNode(info.Id);
#if DEBUG
                        Logger.Write(LogLevel.Debug, $"Retrieved node for connection setup for ID: {info.Id}.",
                            node);
#endif
                        var inputColor = node!.Inputs[i].Color;
                        var outputColor = NodesManager.GetNode(node.Inputs[i].PortInfo.Id)!
                            .Outputs[node.Inputs[i].PortInfo.Index].Color;
                        AddConnector(outputPoint, inputPoint,
                            inputColor, outputColor,
                            new PortInfo(info.Id, i),
                            new PortInfo(info.Connections[i].Id, info.Connections[i].Index));
#if DEBUG
                        Logger.Write(LogLevel.Debug,
                            $"Connector added between node {info.Id} and connection {info.Connections[i].Id}.",
                            new { info.Id, ConnectionIndex = i });
#endif
                        (_nodes[info.Id].InputsPanel.Children[i] as InputPort)!.PortControl.Visibility =
                            Visibility.Hidden;
#if DEBUG
                        Logger.Write(LogLevel.Debug,
                            $"Set visibility hidden for InputPort at node ID: {info.Id}, port index: {i}.");
#endif
                    }
#if DEBUG
                    Logger.Write(LogLevel.Debug, $"Connection setup completed for node ID: {info.Id}.", info);
#endif
                };
            }

            InfoText = "Ready";
#if DEBUG
            Logger.Write(LogLevel.Debug, "BuildNodes completed successfully.");
#endif
        }
        catch (Exception e)
        {
#if DEBUG
            Logger.Write(LogLevel.Error, $"Exception in BuildNodes: {e.Message}", e);
#endif
            InfoText = "Error occurred while building nodes";
            Logger.Write(LogLevel.Error, e.Message, e);
        }
    }

    public async void RebuildNodes(List<NodeInfo> infos)
    {
#if DEBUG
        Logger.Write(LogLevel.Debug, "RebuildNodes started.", new { NodesToRebuildCount = infos.Count });
#endif
        try
        {
            InfoText = "Rebuilding nodes...";
#if DEBUG
            Logger.Write(LogLevel.Debug, "Rebuilding nodes: removing deleted nodes.", infos);
#endif
            // Remove deleted nodes
            var newNodesId = infos.Select(node => node.Id).ToHashSet();
            foreach (var id in _nodes.Select(node => node.Key).Where(id => !newNodesId.Contains(id)))
            {
#if DEBUG
                Logger.Write(LogLevel.Debug, $"Removing node with ID: {id} as it no longer exists in new infos.",
                    id);
#endif
                NodesManager.RemoveNode(id);
            }

#if DEBUG
            Logger.Write(LogLevel.Debug, "Rebuilding nodes: creating new nodes.", infos);
#endif
            // Create new node
            infos.ForEach(info =>
            {
#if DEBUG
                Logger.Write(LogLevel.Debug, $"Processing new node info for ID: {info.Id}.", info);
#endif
                if (_nodes.ContainsKey(info.Id))
                {
#if DEBUG
                    Logger.Write(LogLevel.Debug, $"Node with ID: {info.Id} already exists. Skipping creation.",
                        info);
#endif
                    return;
                }

                var type = info.Type;
                NodeLogic? obj;
                try
                {
#if DEBUG
                    Logger.Write(LogLevel.Debug,
                        $"Attempting to create instance of type {type} using Activator with ItemId.", type);
#endif
                    obj = Activator.CreateInstance(type, ItemId) as NodeLogic;
#if DEBUG
                    Logger.Write(LogLevel.Debug, "Instance creation succeeded using first method.", obj);
#endif
                }
                catch
                {
#if DEBUG
                    Logger.Write(LogLevel.Debug,
                        $"Exception during instance creation for node ID: {info.Id}. Trying alternative constructor.",
                        info);
#endif
                    obj = Activator.CreateInstance(type, []) as NodeLogic;
#if DEBUG
                    Logger.Write(LogLevel.Debug, "Instance creation succeeded using alternative method.", obj);
#endif
                }

                if (obj == null)
                {
#if DEBUG
                    Logger.Write(LogLevel.Debug,
                        $"Instance creation returned null for node ID: {info.Id}. Skipping.", info);
#endif
                    return;
                }

                obj.Id = info.Id;
                NodesManager.AddNode(info.Id, obj);
#if DEBUG
                Logger.Write(LogLevel.Debug, $"New node with ID: {info.Id} added to NodesManager.", obj);
#endif
                for (var i = 0; i < info.Values.Count; i++)
                {
#if DEBUG
                    Logger.Write(LogLevel.Debug,
                        $"Setting input value and connection for node ID: {info.Id}, input index: {i}.",
                        new { Value = info.Values[i], Connection = info.Connections[i] });
#endif
                    obj.SetInput(i, info.Values[i]);
                    obj.SetInputConnection(i, info.Connections[i]);
#if DEBUG
                    Logger.Write(LogLevel.Debug,
                        $"Input value and connection set for node ID: {info.Id}, input index: {i}.");
#endif
                }
            });

#if DEBUG
            Logger.Write(LogLevel.Debug, "Awaiting tasks for connection updates.", infos);
#endif
            await Task.WhenAll(infos.Select(info =>
            {
                return Task.Run(() =>
                {
#if DEBUG
                    Logger.Write(LogLevel.Debug, $"Updating connections in background for node ID: {info.Id}.",
                        info);
#endif
                    if (!_infos.TryGetValue(info.Id, out var value))
                    {
#if DEBUG
                        Logger.Write(LogLevel.Debug, $"No existing info found for node ID: {info.Id} in _infos.",
                            info);
#endif
                        return;
                    }

                    var i = 0;
                    foreach (var connection in value.Connections)
                    {
                        if (connection.Id != info.Connections[i].Id)
                        {
#if DEBUG
                            Logger.Write(LogLevel.Debug,
                                $"Connection mismatch for node ID: {info.Id} at index {i}. Removing and resetting connection.",
                                new
                                {
                                    NodeId = info.Id, Index = i, OldConnection = connection,
                                    NewConnection = info.Connections[i]
                                });
#endif
                            NodesManager.GetNode(info.Id)?.RemoveInputConnection(i);
                            if (info.Connections[i].Id != "")
                                NodesManager.GetNode(info.Id)?.SetInputConnection(i, info.Connections[i]);
#if DEBUG
                            Logger.Write(LogLevel.Debug, $"Connection updated for node ID: {info.Id} at index {i}.",
                                info.Connections[i]);
#endif
                        }

                        i++;
                    }
#if DEBUG
                    Logger.Write(LogLevel.Debug, $"Background connection update completed for node ID: {info.Id}.",
                        info);
#endif
                });
            }));

#if DEBUG
            Logger.Write(LogLevel.Debug, "Clearing _infos dictionary.", _infos);
#endif
            _infos.Clear();
            infos.ForEach(value =>
            {
#if DEBUG
                Logger.Write(LogLevel.Debug, $"Adding node info to _infos for node ID: {value.Id}.", value);
#endif
                _infos.Add(value.Id, value);
            });

#if DEBUG
            Logger.Write(LogLevel.Debug, "Invoking Dispatcher to update UI elements.");
#endif
            Dispatcher.Invoke(() =>
            {
#if DEBUG
                Logger.Write(LogLevel.Debug, "Clearing Canvas and node collections.",
                    new { CanvasChildrenCount = Canvas.Children.Count, NodesCount = _nodes.Count });
#endif
                Canvas.Children.Clear();
                _nodes.Clear();
                _selectingNodes.Clear();
                _connectors.Clear();

#if DEBUG
                Logger.Write(LogLevel.Debug, "Calling BuildNodes from Dispatcher.Invoke.");
#endif
                BuildNodes();
            });
#if DEBUG
            Logger.Write(LogLevel.Debug, "RebuildNodes completed successfully.");
#endif
        }
        catch (Exception e)
        {
#if DEBUG
            Logger.Write(LogLevel.Error, $"Exception in RebuildNodes: {e.Message}", e);
#endif
            InfoText = "Error occurred while rebuilding nodes";
            Logger.Write(LogLevel.Error, e.Message, e);
        }
    }

    internal Point ConvertToTransform(Point p)
    {
        return new Point((p.X - _translateTransform.X) / _scale,
            (p.Y - _translateTransform.Y) / _scale);
    }

    public void AddChildren(Node node, double x, double y)
    {
        Canvas.SetLeft(node, x);
        Canvas.SetTop(node, y);
        Canvas.Children.Add(node);
        var inputs = NodesManager.GetNode(node.Id)?.Inputs ?? [];
        var connections = inputs.Select(t => t.PortInfo).ToList();
        if (!_infos.TryAdd(node.Id, new NodeInfo(node.Id, node.Type, node.Values, x, y, connections)))
            _infos[node.Id] = new NodeInfo(node.Id, node.Type, node.Values, x, y, connections);
        if (!_nodes.TryAdd(node.Id, node))
            _nodes[node.Id] = node;
        node.Moved += (_, _) =>
        {
            _infos[node.Id] = _infos[node.Id] with { X = Canvas.GetLeft(node), Y = Canvas.GetTop(node) };
        };
        node.ValueChanged += (_, _) =>
        {
            try
            {
                List<object?> value = [];
                Dispatcher.Invoke(() =>
                {
                    value.AddRange(from object input in node.InputsPanel.Children select (input as InputPort)?.Value);
                    _infos[node.Id] = _infos[node.Id] with { Values = value };
                    OnNodesUpdated();
                });
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Error, e.Message, e);
            }
        };
    }

    public async void RemoveChildren()
    {
        try
        {
            if (_selectingNodes.Count > 0)
                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var node in _selectingNodes.Where(node =>
                                 node.Type != typeof(InputNode) && node.Type != typeof(OutputNode)))
                    {
                        foreach (OutputPort output in node.OutputsPanel.Children) output.RemoveAllConnection();
                        foreach (InputPort input in node.InputsPanel.Children) input.RemoveConnection();

                        _ = _infos.Select(info =>
                            info.Value.Connections.Where(connection => connection.Id == node.Id)
                                .Select(connection => connection)
                                .Select(connection => info.Value.Connections.Remove(connection)));

                        Canvas.Children.Remove(node);
                        _infos.Remove(node.Id);
                        _nodes.Remove(node.Id);
                        NodesManager.RemoveNode(node.Id);
                    }

                    _selectingNodes.Clear();
                });
            OnNodesUpdated();
        }
        catch (Exception e)
        {
            InfoText = "Error occurred while removing nodes";
            Logger.Write(LogLevel.Error, e.Message, e);
        }
    }

    public void RestoreChild(Node node)
    {
        Canvas.Children.Remove(node);
        Canvas.Children.Add(node);
    }

    public void PreviewConnection(Point pos1, Point pos2)
    {
        Task.Run(async () =>
        {
            if (_previewPath != null) return;
            await Task.Delay(1000);
            if (_previewPath != null)
                await Dispatcher.InvokeAsync(() => InfoText = "Release the mouse button on the port to connect");
        });
        if (_previewPath != null) Canvas.Children.Remove(_previewPath);
        Canvas.Children.Add(_previewPath = new Path()
        {
            Data = new LineGeometry
            {
                StartPoint = ConvertToTransform(pos1),
                EndPoint = ConvertToTransform(pos2)
            },
            Stroke = SystemColors.GrayTextBrush,
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection([5.0, 5.0]),
            IsHitTestVisible = false
        });
    }

    public void RemovePreviewConnection()
    {
        if (_previewPath != null) Canvas.Children.Remove(_previewPath);
        _previewPath = null;
        InfoText = "Ready";
    }

    public void AddConnector(Point pos1, Point pos2, Color col1, Color col2, PortInfo inputPort, PortInfo outputPort)
    {
        try
        {
            InfoText = "Connecting...";
            if (_connectors.ContainsKey((inputPort.Id + ";" + inputPort.Index, outputPort.Id + ";" + outputPort.Index)))
            {
                Canvas.Children.Remove(
                    _connectors[(inputPort.Id + ";" + inputPort.Index, outputPort.Id + ";" + outputPort.Index)]);
                _connectors.Remove((inputPort.Id + ";" + inputPort.Index, outputPort.Id + ";" + outputPort.Index));
            }

            if (_connectors.Where(kvp => kvp.Key.Item1 == inputPort.Id + ";" + inputPort.Index).ToList().Count > 0)
            {
                NodesManager.GetNode(_infos[inputPort.Id].Connections[inputPort.Index].Id)
                    ?.Outputs[_infos[inputPort.Id].Connections[inputPort.Index].Index]
                    .RemoveConnection(inputPort.Id, inputPort.Index);
                RemoveInputConnector(inputPort.Id, inputPort.Index);
                NodesManager.GetNode(inputPort.Id)?.SetInputConnection(inputPort.Index, outputPort);
            }

            Connector connector;
            Canvas.Children.Add(connector = new Connector
            {
                StartPoint = ConvertToTransform(PointFromScreen(pos1)),
                EndPoint = ConvertToTransform(PointFromScreen(pos2)),
                StartColor = col1,
                EndColor = col2,
                IsHitTestVisible = false
            });
            connector.SetValue(Panel.ZIndexProperty, -1);
            _connectors.Add((inputPort.Id + ";" + inputPort.Index, outputPort.Id + ";" + outputPort.Index), connector);
            _infos[inputPort.Id] = _infos[inputPort.Id] with
            {
                Connections = _infos[inputPort.Id].Connections.Select((connection, index) =>
                    index == inputPort.Index ? outputPort : connection).ToList()
            };
            OnNodesUpdated();
            InfoText = "Ready";
        }
        catch (Exception e)
        {
            InfoText = "Error occurred while connecting";
            Logger.Write(LogLevel.Error, e.Message, e);
        }
    }

    public void RemoveInputConnector(string id, int index)
    {
        try
        {
            var connector = _connectors
                .Where(kvp => kvp.Key.Item1 == id + ";" + index)
                .Select(kvp => kvp.Value)
                .ToList()[0];
            Canvas.Children.Remove(connector);
            _connectors.Remove(_connectors
                .Where(kvp => kvp.Value == connector)
                .Select(kvp => kvp.Key)
                .ToList()[0]);
            _infos[id].Connections[index] = new PortInfo();
        }
        catch (Exception e)
        {
            Logger.Write(LogLevel.Error, e.Message, e);
        }
    }

    public void RemoveOutputConnector(string id, int index)
    {
        try
        {
            var connectors = _connectors
                .Where(kvp => kvp.Key.Item2 == id + ";" + index)
                .Select(kvp => kvp.Value)
                .ToList();
            foreach (var connector in connectors)
            {
                var key = _connectors
                    .Where(kvp => kvp.Value == connector)
                    .Select(kvp => kvp.Key)
                    .ToList()[0];
                var sepIndex = key.Item1.IndexOf(';');
                (_nodes[key.Item1[..sepIndex]].InputsPanel
                    .Children[int.Parse(key.Item1[(sepIndex + 1)..])] as InputPort)?.RemoveConnection();
                _infos[key.Item1[..sepIndex]].Connections[int.Parse(key.Item1[(sepIndex + 1)..])] =
                    new PortInfo();
                _connectors.Remove(key);
            }
        }
        catch (Exception e)
        {
            Logger.Write(LogLevel.Error, e.Message, e);
        }
    }

    public void MoveConnector(string id, Point d)
    {
        try
        {
            foreach (var connector in _connectors
                         .Where(kvp => kvp.Key.Item1.StartsWith(id))
                         .Select(kvp => kvp.Value)
                         .ToList())
                connector.EndPoint = new Point(connector.EndPoint.X + d.X, connector.EndPoint.Y + d.Y);
            foreach (var connector in _connectors
                         .Where(kvp => kvp.Key.Item2.StartsWith(id))
                         .Select(kvp => kvp.Value)
                         .ToList())
                connector.StartPoint = new Point(connector.StartPoint.X + d.X, connector.StartPoint.Y + d.Y);
        }
        catch (Exception e)
        {
            Logger.Write(LogLevel.Error, e.Message, e);
        }
    }

    private void UpdateScrollBar()
    {
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (UIElement element in Canvas.Children)
        {
            if (element is not FrameworkElement fe) continue;

            var left = Canvas.GetLeft(fe) * _scale + _translateTransform.X;
            var top = Canvas.GetTop(fe) * _scale + _translateTransform.Y;
            var right = left + fe.ActualWidth * _scale;
            var bottom = top + fe.ActualHeight * _scale;

            if (left < minX) minX = left;
            if (top < minY) minY = top;
            if (right > maxX) maxX = right;
            if (bottom > maxY) maxY = bottom;
        }

        if (Math.Abs(minX - double.MaxValue) < Tolerance || Math.Abs(minY - double.MaxValue) < Tolerance ||
            Math.Abs(maxX - double.MinValue) < Tolerance || Math.Abs(maxY - double.MinValue) < Tolerance)
            _wrapRect = Rect.Empty;
        else
            _wrapRect = new Rect(
                new Point(
                    minX < 0 ? minX : 0,
                    minY < 0 ? minY : 0),
                new Point(
                    maxX > WrapperCanvas.ActualWidth ? maxX : WrapperCanvas.ActualWidth,
                    maxY > WrapperCanvas.ActualHeight ? maxY : WrapperCanvas.ActualHeight
                )
            );

        HorizontalScrollBar.Width = WrapperCanvas.ActualWidth * WrapperCanvas.ActualWidth / _wrapRect.Width;
        Canvas.SetLeft(HorizontalScrollBar, -_wrapRect.Left * WrapperCanvas.ActualWidth / _wrapRect.Width);

        VerticalScrollBar.Height = WrapperCanvas.ActualHeight * WrapperCanvas.ActualHeight / _wrapRect.Height;
        Canvas.SetTop(VerticalScrollBar, -_wrapRect.Top * WrapperCanvas.ActualHeight / _wrapRect.Height);
    }

    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var rate = e.Delta > 0 ? 1.1 : 1 / 1.1;
        var zoom = _scale * rate;
        Canvas_Zoom(zoom, e);
    }

    private void Canvas_Down(object sender, MouseButtonEventArgs e)
    {
        switch (e.ChangedButton)
        {
            case MouseButton.Middle when e.ButtonState == MouseButtonState.Pressed:
                _lastPos = e.GetPosition(this);
                _isDragging = true;
                CaptureMouse();
                break;
            case MouseButton.Left when e.ButtonState == MouseButtonState.Pressed:
                _lastPos = e.GetPosition(this);
                _isSelecting = true;
                _selectingRect =
                    CreateSelectingRectangleFromPoints(ConvertToTransform(_lastPos), ConvertToTransform(_lastPos));
                Canvas.Children.Add(_selectingRect);
                Task.Run(async () =>
                {
                    while (_isSelecting)
                    {
                        var f = GetDirectionIfOutside(_nowPos);
                        Dispatcher.Invoke(() =>
                        {
                            const int step = 15;
                            if ((f & 1) != 0)
                            {
                                _translateTransform.Y += step;
                                _lastPos.Y += step;
                            }

                            if ((f & 2) != 0)
                            {
                                _translateTransform.X -= step;
                                _lastPos.X -= step;
                            }

                            if ((f & 4) != 0)
                            {
                                _translateTransform.Y -= step;
                                _lastPos.Y -= step;
                            }

                            if ((f & 8) != 0)
                            {
                                _translateTransform.X += step;
                                _lastPos.X += step;
                            }

                            Canvas.Children.Remove(_selectingRect);
                            _selectingRect = CreateSelectingRectangleFromPoints(ConvertToTransform(_lastPos),
                                ConvertToTransform(_nowPos));
                            Canvas.Children.Add(_selectingRect);
                        });
                        await Task.Delay(50);
                    }
                });
                CaptureMouse();
                break;
            case MouseButton.Right:
            case MouseButton.XButton1:
            case MouseButton.XButton2:
            default:
                break;
        }
    }

    private void Canvas_Up(object sender, MouseButtonEventArgs e)
    {
        switch (e.ChangedButton)
        {
            case MouseButton.Middle when e.ButtonState == MouseButtonState.Released:
                _isDragging = false;
                ReleaseMouseCapture();
                break;
            case MouseButton.Left when e.ButtonState == MouseButtonState.Released:
            {
                _isSelecting = false;
                Canvas.Children.Remove(_selectingRect);
                foreach (var node in _selectingNodes)
                    Task.Run(() => Dispatcher.Invoke(() =>
                    {
                        node.Width -= 8;
                        node.Height -= 8;
                        Canvas.SetLeft(node, Canvas.GetLeft(node) + 4);
                        Canvas.SetTop(node, Canvas.GetTop(node) + 4);
                        node.Padding = new Thickness(0);
                        node.BorderThickness = new Thickness(0);
                        node.BorderBrush = null;
                    }));
                _selectingNodes = GetElementsInsideRect(Canvas,
                    new Rect(Canvas.GetLeft(_selectingRect),
                        Canvas.GetTop(_selectingRect),
                        _selectingRect.ActualWidth,
                        _selectingRect.ActualHeight));
                _selectingRect = new Rectangle();
                foreach (var node in _selectingNodes)
                    Task.Run(() => Dispatcher.Invoke(() =>
                    {
                        node.Width += 8;
                        node.Height += 8;
                        Canvas.SetLeft(node, Canvas.GetLeft(node) - 4);
                        Canvas.SetTop(node, Canvas.GetTop(node) - 4);
                        node.Padding = new Thickness(2.0);
                        node.BorderThickness = new Thickness(2.0);
                        node.BorderBrush = SystemColors.HighlightBrush;
                    }));
                ReleaseMouseCapture();
                break;
            }
            case MouseButton.Right:
            case MouseButton.XButton1:
            case MouseButton.XButton2:
            default:
                break;
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        _nowPos = e.GetPosition(this);
        if (_isDragging)
        {
            _translateTransform.X += _nowPos.X - _lastPos.X;
            _translateTransform.Y += _nowPos.Y - _lastPos.Y;
            _lastPos = _nowPos;

            UpdateScrollBar();
        }
        else if (_isSelecting)
        {
            Canvas.Children.Remove(_selectingRect);
            _selectingRect =
                CreateSelectingRectangleFromPoints(ConvertToTransform(_lastPos), ConvertToTransform(_nowPos));
            Canvas.Children.Add(_selectingRect);
        }
    }

    public void AllSelect()
    {
        foreach (var node in _selectingNodes)
            Task.Run(() => Dispatcher.Invoke(() =>
            {
                node.Width -= 8;
                node.Height -= 8;
                Canvas.SetLeft(node, Canvas.GetLeft(node) + 4);
                Canvas.SetTop(node, Canvas.GetTop(node) + 4);
                node.Padding = new Thickness(0);
                node.BorderThickness = new Thickness(0);
                node.BorderBrush = null;
            }));
        _selectingNodes = _nodes.Select((kvp) => kvp.Value).ToList();
        foreach (var node in _selectingNodes)
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    node.Width += 8;
                    node.Height += 8;
                    Canvas.SetLeft(node, Canvas.GetLeft(node) - 4);
                    Canvas.SetTop(node, Canvas.GetTop(node) - 4);
                    node.Padding = new Thickness(2.0);
                    node.BorderThickness = new Thickness(2.0);
                    node.BorderBrush = SystemColors.HighlightBrush;
                });
            });
    }

    internal void ToggleSelection(Node selectNode, bool multiple)
    {
        Task.Run(() =>
        {
            if (!multiple)
            {
                var removingSelection = false;
                foreach (var node in _selectingNodes)
                {
                    if (node.Id == selectNode.Id)
                        removingSelection = true;
                    Dispatcher.Invoke(() =>
                    {
                        node.Width -= 8;
                        node.Height -= 8;
                        Canvas.SetLeft(node, Canvas.GetLeft(node) + 4);
                        Canvas.SetTop(node, Canvas.GetTop(node) + 4);
                        node.Padding = new Thickness(0);
                        node.BorderThickness = new Thickness(0);
                        node.BorderBrush = null;
                    });
                }

                if (removingSelection)
                {
                    InfoText = "Ready";
                    _selectingNodes = [];
                    return;
                }

                _selectingNodes = [selectNode];
            }
            else
            {
                if (!_selectingNodes.Contains(selectNode))
                {
                    _selectingNodes.Add(selectNode);
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        _selectingNodes.Remove(selectNode);
                        selectNode.Width -= 8;
                        selectNode.Height -= 8;
                        Canvas.SetLeft(selectNode, Canvas.GetLeft(selectNode) + 4);
                        Canvas.SetTop(selectNode, Canvas.GetTop(selectNode) + 4);
                        selectNode.Padding = new Thickness(0);
                        selectNode.BorderThickness = new Thickness(0);
                        selectNode.BorderBrush = null;
                    });
                    return;
                }
            }

            Dispatcher.Invoke(() =>
            {
                selectNode.Width += 8;
                selectNode.Height += 8;
                Canvas.SetLeft(selectNode, Canvas.GetLeft(selectNode) - 4);
                Canvas.SetTop(selectNode, Canvas.GetTop(selectNode) - 4);
                selectNode.Padding = new Thickness(2.0);
                selectNode.BorderThickness = new Thickness(2.0);
                selectNode.BorderBrush = SystemColors.HighlightBrush;
            });
        });
    }

    private static Rectangle CreateSelectingRectangleFromPoints(Point p1, Point p2)
    {
        var x = Math.Min(p1.X, p2.X);
        var y = Math.Min(p1.Y, p2.Y);
        var width = Math.Abs(p2.X - p1.X);
        var height = Math.Abs(p2.Y - p1.Y);

        var rectangle = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = null,
            Stroke = SystemColors.GrayTextBrush,
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection([5.0, 5.0]),
            IsHitTestVisible = false
        };

        Canvas.SetLeft(rectangle, x);
        Canvas.SetTop(rectangle, y);

        return rectangle;
    }

    internal void MoveNode(Node node, double dx, double dy)
    {
        if (!_selectingNodes.Contains(node))
            ToggleSelection(node, false);
        else
            foreach (var selectedNode in _selectingNodes)
                Task.Run(() => Dispatcher.Invoke(() => selectedNode.Move(dx, dy)));
    }

    internal void SubmitMoving()
    {
        foreach (var selectedNode in _selectingNodes) selectedNode.SubmitMoving();
        OnNodesUpdated();
    }

    private static List<Node> GetElementsInsideRect(Canvas canvas, Rect rect)
    {
        var elementsInsideRect = new List<Node>();

        foreach (UIElement element in canvas.Children)
        {
            if (element is not Node node)
                continue;
            var left = Canvas.GetLeft(node);
            var top = Canvas.GetTop(node);
            var width = node.RenderSize.Width;
            var height = node.RenderSize.Height;

            var elementRect = new Rect(new Point(left, top), new Size(width, height));

            if (rect.Contains(elementRect)) elementsInsideRect.Add(node);
        }

        return elementsInsideRect;
    }

    private int GetDirectionIfOutside(Point p)
    {
        const double left = 0;
        const double top = 0;
        var right = ActualWidth;
        var bottom = ActualHeight;
        var n = 0;
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

    private void Canvas_Zoom(double zoom, Point zoomCenter)
    {
        var rate = zoom switch
        {
            > 4 => 4 / _scale,
            < 0.1 => 0.1 / _scale,
            _ => zoom / _scale
        };

        _scale *= rate;
        _scaleTransform.ScaleX = _scale;
        _scaleTransform.ScaleY = _scale;

        _translateTransform.X += (rate - 1) * (_translateTransform.X - zoomCenter.X);
        _translateTransform.Y += (rate - 1) * (_translateTransform.Y - zoomCenter.Y);

        ZoomValue.Text = (int)(_scale * 100) + "%";
    }

    private void Canvas_Zoom(double zoom)
    {
        var canvasCenter = new Point(WrapperCanvas.ActualWidth / 2, WrapperCanvas.ActualHeight / 2);
        Canvas_Zoom(zoom, canvasCenter);
    }

    private void Canvas_Zoom(double zoom, MouseWheelEventArgs e)
    {
        var mousePosition = e.GetPosition(WrapperCanvas);
        Canvas_Zoom(zoom, mousePosition);
    }

    private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = NumberRegex().IsMatch(e.Text);
    }

    private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            if (e.DataObject.GetData(typeof(string)) is string text && NumberRegex().IsMatch(text)) e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void ZoomValueSubmit(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return) return;
        if (ZoomValue.Text == "") ZoomValue.Text = "100";
        if ("%".EndsWith(ZoomValue.Text)) ZoomValue.Text = ZoomValue.Text.Remove(ZoomValue.Text.Length - 1);
        double zoom;
        try
        {
            zoom = double.Parse(ZoomValue.Text) / 100;
        }
        catch (Exception)
        {
            zoom = 1;
        }

        Canvas_Zoom(zoom);
        Keyboard.ClearFocus();
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex NumberRegex();

    #endregion
}