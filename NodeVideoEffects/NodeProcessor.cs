﻿using NodeVideoEffects.Nodes.Basic;
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
        internal ID2D1DeviceContext6 Context;
        private readonly NodeVideoEffectsPlugin _item;
        public ID2D1Image Output { private set; get; } = null!;

        private readonly OutputNode _outputNode = null!;

        private ID2D1Bitmap1 _bitmap;

        public NodeProcessor(IGraphicsDevicesAndContext context, NodeVideoEffectsPlugin item)
        {
            Context = context.DeviceContext;
            if (item.Id == "" ^ !NodesManager.AddItem(item.Id))
            {
                item.Id = Guid.NewGuid().ToString("N");
                NodesManager.AddItem(item.Id);
                Logger.Write(LogLevel.Info, $"Created the effect processor, ID: \"{item.Id}\".");
            }
            if (item.Nodes.Count == 0)
            {
                var inputNode = new InputNode();
                _outputNode = new OutputNode();
                inputNode.Id = item.Id + "-" + Guid.NewGuid().ToString("N");
                _outputNode.Id = item.Id + "-" + Guid.NewGuid().ToString("N");
                NodesManager.AddNode(inputNode.Id, inputNode);
                NodesManager.AddNode(_outputNode.Id, _outputNode);
                _outputNode.SetInputConnection(0, new Connection(inputNode.Id, 0));
                item.EditorNodes = [
                        new NodeInfo(inputNode.Id, inputNode.GetType(), [], 100, 100, []),
                        new NodeInfo(_outputNode.Id, _outputNode.GetType(), [], 500, 100,
                            [new Connection(inputNode.Id, 0)])
                    ];
                Logger.Write(LogLevel.Info, $"Initializing completed.");
            }
            else
            {
                for (var i = 0; i < item.Nodes.Count; i++)
                {
                    var info = item.Nodes[i];
                    var node = NodesManager.GetNode(info.Id);
                    var index = info.Id.IndexOf('-');
                    if (info.Id[..index] != item.Id)
                        info.Id = item.Id + "-" + info.Id[(index + 1)..];
                    var type = System.Type.GetType(info.Type);
                    if (type != null)
                    {
                        object? obj;
                        try
                        {
                            obj = Activator.CreateInstance(type, item.Id);
                        }
                        catch
                        {
                            obj = Activator.CreateInstance(type, []);
                        }
                        node = obj as NodeLogic ?? throw new Exception("Unable to create node instance.");
                        node.Id = info.Id;
                        NodesManager.AddNode(info.Id, node);
                    }

                    if (node != null)
                    {
                        if (node.GetType() == typeof(OutputNode))
                        {
                            _outputNode = (OutputNode)node;
                        }
                        for (int j = 0; j < info.Values.Count; j++)
                        {
                            node.SetInput(j, info.Values[j]);
                        }
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
                            info.Connections[i] = new Connection(item.Id
                                                                 + "-"
                                                                 + info.Connections[i].Id[(index + 1)..], info.Connections[i].Index);
                        }

                        node?.SetInputConnection(i, info.Connections[i]);
                    }
                }
                Logger.Write(LogLevel.Info, $"Connection restoration completed.");
            }
            NodesManager.SetContext(item.Id, context);

            BitmapProperties1 bitmapProperties = new BitmapProperties1(
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

            this._item = item;
        }

        public void SetInput(ID2D1Image? input)
        {
            NodesManager.SetInput(_item.Id, input);
            var output = ((ImageWrapper?)_outputNode.Inputs[0].Value)?.Image ?? null;
            if (output == null || output.NativePointer == 0)
            {
                Logger.Write(LogLevel.Warn, $"The output of this item(ID: \"{_item.Id}\" is a blank image because the output of OutputNode is null.");

                SetBlankImage(out output);
            }
            Output = output;
        }

        public void ClearInput() { }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            try
            {
                typeof(NodesManager).GetMethod("SetInfo",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    ?.Invoke(null, [_item.Id, effectDescription]);
                var output = ((ImageWrapper?)_outputNode.Inputs[0].Value)?.Image ?? null;
                if (output == null || output.NativePointer == 0)
                {
                    Logger.Write(LogLevel.Warn, $"The output of this item(ID: \"{_item.Id}\" is a blank image because the output of OutputNode is null.");

                    SetBlankImage(out output);
                }
                Output = output;
            }
            catch (NullReferenceException e)
            {
                Logger.Write(LogLevel.Error, $"{e.Message}\nItem ID: \"{_item.Id}\"");

                SetBlankImage(out var output);
                Output = output;
            }

            return effectDescription.DrawDescription;
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
            try
            {
                Output.Dispose();
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Warn, $"{e.Message}\nItem ID: \"{_item.Id}\"");
            }
        }
    }
}
