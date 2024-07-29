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
        string nodes;

        Nodes.Basic.InputNode inputNode = new();
        Nodes.Basic.OutputNode outputNode = new();

        public NodeProcessor(IGraphicsDevicesAndContext context, NodeVideoEffectsPlugin item)
        {
            _context = context.DeviceContext;
            outputNode.SetInputConnection(0, new(inputNode.Id, 0));
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
            isFirst = false;

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
