using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using System.Reflection;
using Vortice.Direct2D1;
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
            if (item.Nodes.Count == 0)
            {
                inputNode = new();
                outputNode = new();
                outputNode.SetInputConnection(0, new(inputNode.Id, 0));
                item.Nodes.Add(new(inputNode.Id, inputNode.GetType(), [], 100, 100, []));
                item.Nodes.Add(new(outputNode.Id, outputNode.GetType(), [], 500, 100, [new(inputNode.Id, 0)]));
            }
            else
            {
                foreach (NodeInfo info in item.Nodes)
                {
                    INode? node;
                    if ((node = NodesManager.GetNode(info.ID)) == null)
                    {
                        System.Type? type = System.Type.GetType(info.Type);
                        if (type != null)
                        {
                            object? obj = Activator.CreateInstance(type, [info.ID]);

                            node = obj as INode;
                        }
                    }

                    if (node != null)
                    {
                        if (node.GetType() == typeof(InputNode))
                            inputNode = (InputNode)node;
                        if (node.GetType() == typeof(OutputNode))
                            outputNode = (OutputNode)node;
                    }
                }
                foreach (NodeInfo info in item.Nodes)
                {
                    INode node = NodesManager.GetNode(info.ID);
                    for (int i = 0; i < info.Connections.Count; i++)
                    {
                        node.SetInputConnection(i, info.Connections[i]);
                    }
                }
            }
        
            this.item = item;
        }

        public void SetInput(ID2D1Image input)
        {
            inputNode.Outputs[0].Value = new ImageAndContext(input, _context);
            ID2D1Image _output = ((ImageAndContext)outputNode.Inputs[0].Value).Image;
            Output = _output;
        }

        public void ClearInput()
        {
            inputNode.Outputs[0].Value = null;
        }

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
