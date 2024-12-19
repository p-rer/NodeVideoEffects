using NodeVideoEffects.Type;
using SharpGen.Runtime;
using System.Windows.Media;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect
{
    public class GaussianBlurNode : INode
    {
        GaussianBlur? blur;
        string id;
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
            if (id == "") return;
            this.id = id;
        }

        public override async Task Calculate()
        {
            if (blur == null || blur.NativePointer == 0)
            {
                blur = new(NodesManager.GetContext(id).DeviceContext);
            }
            lock (blur)
            {
                blur.SetInput(0, ((ImageAndContext)Inputs[0].Value).Image, true);
                blur.StandardDeviation = Convert.ToSingle(Inputs[1].Value);
                Outputs[0].Value = new ImageAndContext(blur.Output);
            }
            return;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (blur != null)
            {
                blur?.SetInput(0, null, true);
                blur?.Dispose();
                blur = null;
            }
        }
    }
}
