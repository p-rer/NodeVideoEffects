using NodeVideoEffects.Editor;
using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using System;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
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
        ID2D1DeviceContext6 _context;
        readonly NodeVideoEffectsPlugin item;
        public ID2D1Image Output { set; get; }

        bool isFirst = true;

        Nodes.Basic.InputNode inputNode;
        Nodes.Basic.OutputNode outputNode;

        public NodeProcessor(IGraphicsDevicesAndContext context, NodeVideoEffectsPlugin item)
        {
            _context = context.DeviceContext;
            if (item.ID == "" ^ !NodesManager.AddItem(item.ID))
            {
                item.ID = Guid.NewGuid().ToString("N");
                NodesManager.AddItem(item.ID);
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
            }
            else
            {
                foreach (NodeInfo info in item.Nodes)
                {
                    INode? node = NodesManager.GetNode(info.ID);
                    int index = info.ID.IndexOf('-');
                    if (info.ID[0..index] != item.ID) info.ID = item.ID + "-" + info.ID[(index + 1)..^0];
                    System.Type? type = System.Type.GetType(info.Type);
                    if (type != null)
                    {
                        object? obj = Activator.CreateInstance(type, []);
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
                        if(info.Connections[i].id != "")
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
            }
        
            this.item = item;
        }

        public void SetInput(ID2D1Image input)
        {
            inputNode.Outputs[0].Value = new ImageAndContext(input, _context);
            ID2D1Image? _output = ((ImageAndContext?)outputNode.Inputs[0].Value)?.Image ?? null;
            if (_output == null)
            {
                BitmapProperties1 bitmapProperties = new BitmapProperties1(
                    new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                    96,
                    96,
                    BitmapOptions.Target
                );

                ID2D1Bitmap1 bitmap = _context.CreateBitmap(
                    new SizeI(1, 1),
                    IntPtr.Zero,
                    0,
                    bitmapProperties
                );

                _context.Target = bitmap;
                _context.BeginDraw();
                _context.Clear(new Color(0, 0, 0, 0));
                _context.EndDraw();

                _output = bitmap;
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
                    .GetMethod("SetInfo",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Invoke(null, [effectDescription]);
                ID2D1Image? _output = ((ImageAndContext?)outputNode.Inputs[0].Value)?.Image??null;
                if (_output == null)
                {
                    BitmapProperties1 bitmapProperties = new BitmapProperties1(
                        new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                        96,
                        96,
                        BitmapOptions.Target
                    );

                    ID2D1Bitmap1 bitmap = _context.CreateBitmap(
                        new SizeI(1, 1),
                        IntPtr.Zero,
                        0,
                        bitmapProperties
                    );

                    _context.Target = bitmap;
                    _context.BeginDraw();
                    _context.Clear(new Color(0, 0, 0, 0));
                    _context.EndDraw();

                    _output = bitmap;
                }
                Output = _output;
            }
            catch (NullReferenceException e)
            {

            }

            return effectDescription.DrawDescription;
        }

        public void Dispose()
        {
            ClearInput();
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
