using NodeVideoEffects.Type;
using System.Reflection;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects
{
    internal class NodeProcessor : IVideoEffectProcessor
    {
        readonly NodeVideoEffectsPlugin item;
        private ID2D1Image Input;
        public ID2D1Image Output { set; get; }

        bool isFirst = true;
        string nodes;

        public NodeProcessor(IGraphicsDevicesAndContext devices, NodeVideoEffectsPlugin item)
        {
            this.item = item;
        }

        public void SetInput(ID2D1Image input)
        {
            Output = input;
        }

        public void ClearInput()
        {
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
            } catch (NullReferenceException e)
            {

            }
            isFirst = false;

            return effectDescription.DrawDescription;
        }

        public void Dispose()
        {
            ClearInput();
            if (Output != null) Output.Dispose();
        }
    }
}
