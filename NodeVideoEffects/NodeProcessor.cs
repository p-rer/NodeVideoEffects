using System.Numerics;
using NodeVideoEffects.Core;
using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Utility;
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
    private readonly InputNode _inputNode = null!;
    private readonly NodeVideoEffectsPlugin _item;

    private readonly Lock _locker = new();

    private readonly OutputNode _outputNode = null!;
    private AffineTransform2D? _affineTransform2D;

    private ID2D1Bitmap1? _bitmap;
    private bool _hasError;
    private bool _isCalculating;
    internal ID2D1DeviceContext6 Context;

    public NodeProcessor(IGraphicsDevicesAndContext context, NodeVideoEffectsPlugin item)
    {
        _item = item;

        if (!NodesManager.AddItem(item.Id))
        {
            item.Id = Guid.NewGuid().ToString("N");
            NodesManager.AddItem(item.Id);
        }

        Context = context.DeviceContext;
        NodesManager.SetContext(item.Id, context);

        if (item.Nodes.Count == 0)
        {
            _outputNode = new OutputNode();
            _inputNode = new InputNode();
            _inputNode.Id = item.Id + "-" + Guid.NewGuid().ToString("N");
            _outputNode.Id = item.Id + "-" + Guid.NewGuid().ToString("N");
            NodesManager.AddNode(_inputNode.Id, _inputNode);
            NodesManager.AddNode(_outputNode.Id, _outputNode);
            _outputNode.SetInputConnection(0, new PortInfo(_inputNode.Id, 0));
            item.EditorNodes =
            [
                new NodeInfo(_inputNode.Id, _inputNode.GetType(), [], 100, 100, []),
                new NodeInfo(_outputNode.Id, _outputNode.GetType(), [], 500, 100,
                    [new PortInfo(_inputNode.Id, 0)])
            ];
        }
        else
        {
            for (var i = 0; i < item.Nodes.Count; i++)
            {
                var info = item.Nodes[i];
                info = item.Nodes[i] = info with { Id = item.Id + "-" + info.Id[(info.Id.IndexOf('-') + 1)..] };
                var type = info.Type;
                object? obj;
                try
                {
                    obj = Activator.CreateInstance(type, item.Id);
                }
                catch
                {
                    obj = Activator.CreateInstance(type, []);
                }

                var node = obj as NodeLogic ?? throw new Exception("Unable to create node instance.");
                node.Id = info.Id;
                NodesManager.AddNode(info.Id, node);
                if (node.GetType() == typeof(OutputNode))
                {
                    _outputNode = (OutputNode)node;
                }

                if (node.GetType() == typeof(InputNode))
                {
                    _inputNode = (InputNode)node;
                }

                for (var j = 0; j < info.Values.Count; j++)
                {
                    node.SetInput(j, info.Values[j]);
                }
            }

            foreach (var info in item.Nodes)
            {
                var node = NodesManager.GetNode(info.Id);
                for (var i = 0; i < info.Connections.Count; i++)
                {
                    if (info.Connections[i].Id == "") continue;
                    var index = info.Id.IndexOf('-');
                    if (info.Connections[i].Id[..index] != item.Id)
                    {
                        info.Connections[i] = new PortInfo(item.Id
                                                           + "-"
                                                           + info.Connections[i].Id[(index + 1)..],
                            info.Connections[i].Index);
                    }

                    node?.SetInputConnection(i, info.Connections[i]);
                }
            }

            Logger.Write(LogLevel.Info, "Connection restoration completed.");
        }

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

        Logger.Write(LogLevel.Info, "NodeProcessor initialized successfully.");
    }

    public ID2D1Image Output { private set; get; } = null!;

    public void SetInput(ID2D1Image? input)
    {
        lock (_locker)
        {
            Logger.Write(LogLevel.Debug,
                $"Setting input image: 0x{input?.NativePointer.ToString("x8") ?? "null"}");
            _inputNode.Image = input;
        }
    }

    public void ClearInput()
    {
        lock (_locker)
        {
            _inputNode.Image = null;
        }
    }

    public DrawDescription Update(EffectDescription effectDescription)
    {
        lock (_locker)
        {
            ID2D1Image? output = null;
            try
            {
                NodesManager.SetInfo(_item.Id, effectDescription);
                if (_isCalculating)
                {
                    return effectDescription.DrawDescription;
                }

                _isCalculating = true;
                output = ((ImageWrapper?)_outputNode.GetOutput(0))?.Image;
                Logger.Write(LogLevel.Debug,
                    $"Output image retrieved: 0x{output?.NativePointer.ToString("x8") ?? "null"}");
                if (output == null || output.NativePointer == 0)
                    throw new InvalidOperationException("Output image is null.");
                _hasError = false;
                return effectDescription.DrawDescription;
            }
            catch (Exception e)
            {
                if (_hasError == false)
                    Logger.Write(LogLevel.Error, $"{e.Message}\nItem ID: \"{_item.Id}\"", e);
                _hasError = true;

                SetBlankImage(out output);
                return effectDescription.DrawDescription;
            }
            finally
            {
                _affineTransform2D ??= new AffineTransform2D(Context)
                    { BorderMode = BorderMode.Soft, TransformMatrix = Matrix3x2.Identity };
                _affineTransform2D.SetInput(0, output, true);

                _isCalculating = false;
                Output = _affineTransform2D.Output;
            }
        }
    }

    public void Dispose()
    {
        Logger.Write(LogLevel.Debug, "Disposing NodeProcessor resources.");
        ClearInput();
        _bitmap?.Dispose();
        _bitmap = null;
        _affineTransform2D?.SetInput(0, null, true);
        _affineTransform2D?.Dispose();
        _affineTransform2D = null;
        Output.Dispose();
    }

    internal void UpdateContext(IGraphicsDevicesAndContext context)
    {
        Context = context.DeviceContext;
        NodesManager.SetContext(_item.Id, context);
    }

    private void SetBlankImage(out ID2D1Image output)
    {
        if (_bitmap == null)
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
}