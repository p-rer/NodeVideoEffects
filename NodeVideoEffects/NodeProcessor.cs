using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using NodeVideoEffects.Utility;
using System.Reflection;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.DXGI;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects
{
    internal class NodeProcessor : IVideoEffectProcessor
    {
        internal ID2D1DeviceContext6 _context;
        readonly NodeVideoEffectsPlugin item;
        public ID2D1Image Output { set; get; }

        Nodes.Basic.InputNode inputNode;
        Nodes.Basic.OutputNode outputNode;

        ID2D1Bitmap1 bitmap;

        public NodeProcessor(IGraphicsDevicesAndContext context, NodeVideoEffectsPlugin item)
        {
            _context = context.DeviceContext;
            if (item.ID == "" ^ !NodesManager.AddItem(item.ID))
            {
                item.ID = Guid.NewGuid().ToString("N");
                NodesManager.AddItem(item.ID);
                Logger.Write(LogLevel.Info, $"Created the effect processor, ID: \"{item.ID}\".");
            }
            if (item.Nodes.Count == 0)
            {
                inputNode = new();
                outputNode = new();
                inputNode.Id = item.ID + "-" + Guid.NewGuid().ToString("N");
                outputNode.Id = item.ID + "-" + Guid.NewGuid().ToString("N");
                NodesManager.AddNode(inputNode.Id, inputNode);
                NodesManager.AddNode(outputNode.Id, outputNode);
                outputNode.SetInputConnection(0, new(inputNode.Id, 0));
                item.Nodes.Add(new(inputNode.Id, inputNode.GetType(), [], 100, 100, []));
                item.Nodes.Add(new(outputNode.Id, outputNode.GetType(), [], 500, 100, [new(inputNode.Id, 0)]));
                Logger.Write(LogLevel.Info, $"Initializing completed.");
            }
            else
            {
                foreach (NodeInfo info in item.Nodes)
                {
                    INode? node = NodesManager.GetNode(info.ID);
                    int index = info.ID.IndexOf('-');
                    if (info.ID[0..index] != item.ID)
                        info.ID = item.ID + "-" + info.ID[(index + 1)..^0];
                    System.Type? type = System.Type.GetType(info.Type);
                    if (type != null)
                    {
                        object? obj;
                        try
                        {
                            obj = Activator.CreateInstance(type, [item.ID]);
                        }
                        catch
                        {
                            obj = Activator.CreateInstance(type, []);
                        }
                        node = obj as INode ?? throw new Exception("Unable to create node instance.");
                        node.Id = info.ID;
                        NodesManager.AddNode(info.ID, node);
                    }

                    if (node != null)
                    {
                        if (node.GetType() == typeof(InputNode))
                            inputNode = (InputNode)node;
                        if (node.GetType() == typeof(OutputNode))
                        {
                            outputNode = (OutputNode)node;
                        }
                        for (int i = 0; i < info.Values.Count; i++)
                        {
                            node.SetInput(i, info.Values[i]);
                        }
                    }
                }
                foreach (NodeInfo info in item.Nodes)
                {
                    INode? node = NodesManager.GetNode(info.ID);
                    for (int i = 0; i < info.Connections.Count; i++)
                    {
                        if (info.Connections[i].id != "")
                        {
                            int index = info.ID.IndexOf('-');
                            if (info.Connections[i].id[0..index] != item.ID)
                            {
                                info.Connections[i] = new(item.ID
                                                          + "-"
                                                          + info.Connections[i].id[(index + 1)..^0], info.Connections[i].index);
                            }

                            node?.SetInputConnection(i, info.Connections[i]);
                        }
                    }
                }
                Logger.Write(LogLevel.Info, $"Connection restoration completed.");
            }
            NodesManager.SetContext(item.ID, context);

            BitmapProperties1 bitmapProperties = new BitmapProperties1(
                new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                96,
                96,
                BitmapOptions.Target
            );

            bitmap = _context.CreateBitmap(
                new SizeI(1, 1),
                IntPtr.Zero,
                0,
                bitmapProperties
            );

            this.item = item;
        }

        public void SetInput(ID2D1Image input)
        {
            NodesManager.SetInput(item.ID, input);
            ID2D1Image? _output = ((ImageAndContext?)outputNode.Inputs[0].Value)?.Image ?? null;
            if (_output == null || _output.NativePointer == 0)
            {
                Logger.Write(LogLevel.Warn, $"The output of this item(ID: \"{item.ID}\" is a blank image because the output of OutputNode is null.");

                SetBlankImage(out _output);
            }
            Output = _output;
        }

        public void ClearInput() { }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;
            try
            {
                typeof(NodesManager)
                    ?.GetMethod("SetInfo",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    ?.Invoke(null, [item.ID, effectDescription]);
                ID2D1Image? _output = ((ImageAndContext?)outputNode.Inputs[0].Value)?.Image ?? null;
                if (_output == null || _output.NativePointer == 0)
                {
                    Logger.Write(LogLevel.Warn, $"The output of this item(ID: \"{item.ID}\" is a blank image because the output of OutputNode is null.");

                    SetBlankImage(out _output);
                }
                Output = _output;
            }
            catch (NullReferenceException e)
            {
                Logger.Write(LogLevel.Error, $"{e.Message}\nItem ID: \"{item.ID}\"");

                ID2D1Image _output;
                SetBlankImage(out _output);
                Output = _output;
            }

            return effectDescription.DrawDescription;
        }

        private void SetBlankImage(out ID2D1Image _output)
        {
            if (bitmap.NativePointer == 0)
            {
                BitmapProperties1 bitmapProperties = new BitmapProperties1(
                new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                96,
                96,
                BitmapOptions.Target
            );

                bitmap = _context.CreateBitmap(
                    new SizeI(1, 1),
                    IntPtr.Zero,
                    0,
                    bitmapProperties
                );
            }
            _context.Target = bitmap;
            _context.BeginDraw();
            _context.Clear(new Color(0, 0, 0, 0));
            _context.EndDraw();

            _output = bitmap;
        }

        public void Dispose()
        {
            ClearInput();
            bitmap.Dispose();
            try
            {
                Output.Dispose();
            }
            catch
            {

            }
        }
    }
}
