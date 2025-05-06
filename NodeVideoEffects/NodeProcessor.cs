using System.Numerics;
using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using System.Reflection;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using Vortice.DXGI;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects;

internal class NodeProcessor : IVideoEffectProcessor
{
    internal ID2D1DeviceContext6 Context;
    private readonly NodeVideoEffectsPlugin _item;

    public ID2D1Image Output { private set; get; } = null!;

    private readonly OutputNode _outputNode = null!;

    private ID2D1Bitmap1 _bitmap;
    private AffineTransform2D? _affineTransform2D;

    private readonly Lock _locker = new();
    private bool _isCalculating;
    private bool _hasError;

    public NodeProcessor(IGraphicsDevicesAndContext context, NodeVideoEffectsPlugin item)
    {
        Logger.Write(LogLevel.Debug, "NodeProcessor constructor started.");
        Logger.Write(LogLevel.Debug,
            $"Received context: {context.ToString() ?? "null"} and plugin item with ID: \"{item.Id}\".");

        if ((item.Id == "") ^ !NodesManager.AddItem(item.Id))
        {
            Logger.Write(LogLevel.Debug,
                "Condition met: item.Id is empty or NodesManager.AddItem(item.Id) failed.");
            item.Id = Guid.NewGuid().ToString("N");
            Logger.Write(LogLevel.Debug, $"Generated new ID: \"{item.Id}\". Adding to NodesManager.");
            NodesManager.AddItem(item.Id);
            Logger.Write(LogLevel.Debug, $"Created the effect processor, ID: \"{item.Id}\".");
        }

        Logger.Write(LogLevel.Debug, "Setting the DeviceContext.");
        Context = context.DeviceContext;
        NodesManager.SetContext(item.Id, context);
        Logger.Write(LogLevel.Debug, "Context has been set in NodesManager.");

        if (item.Nodes.Count == 0)
        {
            Logger.Write(LogLevel.Debug,
                "No nodes exist in item.Nodes. Generating initial nodes (InputNode and OutputNode).");
            var inputNode = new InputNode();
            _outputNode = new OutputNode();
            inputNode.Id = item.Id + "-" + Guid.NewGuid().ToString("N");
            _outputNode.Id = item.Id + "-" + Guid.NewGuid().ToString("N");
            Logger.Write(LogLevel.Debug,
                $"Generated InputNode ID: \"{inputNode.Id}\", OutputNode ID: \"{_outputNode.Id}\".");
            NodesManager.AddNode(inputNode.Id, inputNode);
            NodesManager.AddNode(_outputNode.Id, _outputNode);
            Logger.Write(LogLevel.Debug, "Nodes added to NodesManager.");
            _outputNode.SetInputConnection(0, new PortInfo(inputNode.Id, 0));
            Logger.Write(LogLevel.Debug, "OutputNode input connection set with InputNode.");
            item.EditorNodes =
            [
                new NodeInfo(inputNode.Id, inputNode.GetType(), [], 100, 100, new List<PortInfo>()),
                new NodeInfo(_outputNode.Id, _outputNode.GetType(), [], 500, 100,
                    [new PortInfo(inputNode.Id, 0)])
            ];
            Logger.Write(LogLevel.Info, "Initializing completed.");
            Logger.Write(LogLevel.Debug, "EditorNodes set successfully.");
        }
        else
        {
            Logger.Write(LogLevel.Debug,
                "Existing nodes detected in item.Nodes. Starting node regeneration process.");
            for (var i = 0; i < item.Nodes.Count; i++)
            {
                var info = item.Nodes[i];
                info = item.Nodes[i] = info with { Id = item.Id + "-" + info.Id[(info.Id.IndexOf('-') + 1)..] };
                Logger.Write(LogLevel.Debug, $"Updated node ID to \"{info.Id}\".");
                var type = info.Type;
                object? obj;
                try
                {
                    Logger.Write(LogLevel.Debug,
                        $"Attempting to create instance of type {type} using Activator with item.Id.");
                    obj = Activator.CreateInstance(type, item.Id);
                    Logger.Write(LogLevel.Debug, "Instance creation succeeded using first method.");
                }
#pragma warning disable CS0168
                catch (Exception ex)
#pragma warning restore CS0168
                {
                    Logger.Write(LogLevel.Debug,
                        $"Exception during Activator.CreateInstance: {ex.Message}. Trying alternative constructor.");
                    obj = Activator.CreateInstance(type, []);
                    Logger.Write(LogLevel.Debug, "Instance creation succeeded using alternative method.");
                }

                var node = obj as NodeLogic ?? throw new Exception("Unable to create node instance.");
                node.Id = info.Id;
                NodesManager.AddNode(info.Id, node);
                Logger.Write(LogLevel.Debug, $"Node {node.Id} added to NodesManager.");
                if (node.GetType() == typeof(OutputNode))
                {
                    Logger.Write(LogLevel.Debug, "OutputNode found and assigned to _outputNode.");
                    _outputNode = (OutputNode)node;
                }

                for (var j = 0; j < info.Values.Count; j++)
                {
                    node.SetInput(j, info.Values[j]);
                    Logger.Write(LogLevel.Debug, $"Set input {j} for node {node.Id}.");
                }
            }

            foreach (var info in item.Nodes)
            {
                var node = NodesManager.GetNode(info.Id);
                Logger.Write(LogLevel.Debug, $"Retrieved node {info.Id} from NodesManager.");
                for (var i = 0; i < info.Connections.Count; i++)
                {
                    if (info.Connections[i].Id == "") continue;
                    var index = info.Id.IndexOf('-');
                    if (info.Connections[i].Id[..index] != item.Id)
                    {
                        Logger.Write(LogLevel.Debug,
                            $"Updating connection ID from \"{info.Connections[i].Id}\" to match item.Id.");
                        info.Connections[i] = new PortInfo(item.Id
                                                           + "-"
                                                           + info.Connections[i].Id[(index + 1)..],
                            info.Connections[i].Index);
                        Logger.Write(LogLevel.Debug, $"Connection ID updated to \"{info.Connections[i].Id}\".");
                    }

                    node?.SetInputConnection(i, info.Connections[i]);
                    Logger.Write(LogLevel.Debug, $"Set connection {i} for node {info.Id}.");
                }
            }
            Logger.Write(LogLevel.Info, "Connection restoration completed.");
            Logger.Write(LogLevel.Debug, "All node connections have been successfully restored.");
        }

        Logger.Write(LogLevel.Debug, "Starting Bitmap properties configuration.");
        var bitmapProperties = new BitmapProperties1(
            new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
            96,
            96,
            BitmapOptions.Target
        );
        Logger.Write(LogLevel.Debug, "Bitmap properties configured successfully.");
        Logger.Write(LogLevel.Debug, "Creating Bitmap.");
        _bitmap = Context.CreateBitmap(
            new SizeI(1, 1),
            IntPtr.Zero,
            0,
            bitmapProperties
        );
        Logger.Write(LogLevel.Debug, "Bitmap created successfully.");

        _item = item;
        Logger.Write(LogLevel.Debug, "NodeProcessor constructor processing completed.");
    }

    public void SetInput(ID2D1Image? input)
    {
        lock (_locker)
        {
            Logger.Write(LogLevel.Debug, $"Setting input for node processor.\nID: {_item.Id}\nInput: 0x{input?.NativePointer??0:x8}");
            NodesManager.SetInput(_item.Id, input);
        }
    }

    public void ClearInput()
    {
    }

    public DrawDescription Update(EffectDescription effectDescription)
    {
        lock (_locker)
        {
            ID2D1Image? output = null;
            try
            {
                typeof(NodesManager).GetMethod("SetInfo",
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static |
                        BindingFlags.FlattenHierarchy)
                    ?.Invoke(null, [_item.Id, effectDescription]);
                if (effectDescription.Usage != TimelineSourceUsage.Exporting && _isCalculating)
                {
                    return effectDescription.DrawDescription;
                }
                _isCalculating = true;
                output = (
                    (ImageWrapper?)NodesManager.GetOutputValue(_outputNode.Inputs[0].PortInfo.Id, _outputNode.Inputs[0].PortInfo.Index)
                        .GetAwaiter()
                        .GetResult())?.Image;
                Logger.Write(LogLevel.Debug,
                    $"Output image retrieved: 0x{output?.NativePointer.ToString("x8") ?? "null"}");
                if (output == null || output.NativePointer == 0)
                    throw new InvalidOperationException("Output image is null.");
                _isCalculating = false;
                _hasError = false;
                return effectDescription.DrawDescription;
            }
            catch (Exception e)
            {
                if (_hasError == false)
                    Logger.Write(LogLevel.Error, $"{e.Message}\nItem ID: \"{_item.Id}\"", e);
                _isCalculating = false;
                _hasError = true;

                SetBlankImage(out output);
                return effectDescription.DrawDescription;
            }
            finally
            {
                if (_affineTransform2D == null || _affineTransform2D.NativePointer == 0)
                    _affineTransform2D = new AffineTransform2D(Context)
                        { BorderMode = BorderMode.Soft, TransformMatrix = Matrix3x2.Identity };
                _affineTransform2D ??= new AffineTransform2D(Context)
                    { BorderMode = BorderMode.Soft, TransformMatrix = Matrix3x2.Identity };
                _affineTransform2D.SetInput(0, output, true);

                Output = _affineTransform2D.Output;
            }
        }
    }

    private void SetBlankImage(out ID2D1Image output)
    {
        if (_bitmap.NativePointer == 0)
        {
            var bitmapProperties = new BitmapProperties1(
                new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                96,
                96,
                BitmapOptions.Target
            );

            _bitmap = Context.CreateBitmap(
                new SizeI(1, 1),
                IntPtr.Zero,
                0,
                bitmapProperties
            );
        }

        Context.Target = _bitmap;
        Context.BeginDraw();
        Context.Clear(new Color(0, 0, 0, 0));
        Context.EndDraw();

        output = _bitmap;
    }

    public void Dispose()
    {
        ClearInput();
        _bitmap.Dispose();
        _affineTransform2D?.SetInput(0, null, true);
        _affineTransform2D?.Dispose();
        Output.Dispose();
    }
}