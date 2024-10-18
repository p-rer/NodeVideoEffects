using NodeVideoEffects.Type;
using System.Windows.Media;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect
{
    public class GaussianBlurNode : INode
    {
        GaussianBlur blur;
        public GaussianBlurNode() : base(
            [
                new(new Image(null), "In"),
                new(new Number(10, 0, 250,4), "Level")
            ],
            [
                new(new Image(null), "Out")
            ],
            "Gaussian Blur",
            Colors.LawnGreen,
            "Effect")
        {
        }

        public override async Task Calculate()
        {
            ID2D1DeviceContext6 context = ((ImageAndContext)Inputs[0].Value).Context;
            blur = new(context);
            blur.SetInput(0, ((ImageAndContext)Inputs[0].Value).Image, true);
            blur.StandardDeviation = Convert.ToSingle(Inputs[1].Value);
            Outputs[0].Value = new ImageAndContext(blur.Output, context);
            return;
        }

        public override void Dispose()
        {
            blur.SetInput(0, null, true);
            blur.Dispose();
        }
    }
}
