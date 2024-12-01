using NodeVideoEffects.Type;
using System.Windows.Media;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect
{
    public class GaussianBlurNode : INode
    {
        GaussianBlur? blur;
        public GaussianBlurNode(string id) : base(
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
            if (id == "")
                return;
            blur = new(NodesManager.GetContext(id).DeviceContext);
        }

        public override async Task Calculate()
        {
            if (blur == null)
                return;
            blur.SetInput(0, ((ImageAndContext)Inputs[0].Value).Image, true);
            blur.StandardDeviation = Convert.ToSingle(Inputs[1].Value);
            Outputs[0].Value = new ImageAndContext(blur.Output, ((ImageAndContext)Inputs[0].Value).Context);
            return;
        }

        public override void Dispose()
        {
            base.Dispose();
            blur?.SetInput(0, null, true);
            blur?.Dispose();
        }
    }
}
