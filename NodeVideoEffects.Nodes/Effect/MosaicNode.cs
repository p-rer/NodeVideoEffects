using NodeVideoEffects.Type;
using NodeVideoEffects.Utility;
using System.Windows.Media;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Nodes.Effect
{
    public class MosaicNode : INode
    {
        VideoEffectsLoader videoEffect;
        public MosaicNode(string id) : base(
            [
                new(new Image(null), "In"),
                new(new Number(10, 0, 250,4), "Level")
            ],
            [
                new(new Image(null), "Out")
            ],
            "Mosaic",
            Colors.LawnGreen,
            "Effect")
        {
            if (id == "")
                return;
            videoEffect = VideoEffectsLoader.LoadEffect("MosaicEffect", id);
        }

        public override async Task Calculate()
        {
            if (videoEffect.Update(((ImageAndContext)Inputs[0].Value).Image, out ID2D1Image? output))
            {
                Outputs[0].Value = new ImageAndContext(output);
            }
            return;
        }

        public override void Dispose()
        {
            base.Dispose();
            videoEffect?.Dispose();
        }
    }
}