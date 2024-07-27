using NodeVideoEffects.Type;
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
                    .GetMethod("SetFrame", 
                    System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Static)
                    .Invoke(null, [frame]);
                typeof(NodesManager)
                    .GetMethod("SetLength",
                    System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Static)
                    .Invoke(null, [length]);
                typeof(NodesManager)
                    .GetMethod("SetFPS",
                    System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Static)
                    .Invoke(null, [fps]);
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
