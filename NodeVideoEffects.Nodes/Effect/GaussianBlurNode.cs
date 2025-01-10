using NodeVideoEffects.Type;
using System.Windows.Media;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect
{
    public class GaussianBlurNode : INode
    {
        private GaussianBlur _blur = null!;

        public GaussianBlurNode(string id) : base(
            [
                new Input(new Image(null), "In"),
                new Input(new Number(10, 0, 250,4), "Level")
            ],
            [
                new Output(new Image(null), "Out")
            ],
            "Gaussian Blur",
            Colors.LawnGreen,
            "Effect")
        {
            if (id == "") return;
            _blur = new GaussianBlur(NodesManager.GetContext(id).DeviceContext);
        }

        public override async Task Calculate()
        {
            await Task.Run(() =>
            {
                lock (_blur)
                {
                    _blur.SetInput(0, ((ImageAndContext?)Inputs[0].Value)?.Image, true);
                    _blur.StandardDeviation = Convert.ToSingle(Inputs[1].Value);
                    Outputs[0].Value = new ImageAndContext(_blur.Output);
                }
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_blur == null!) return;
            _blur.SetInput(0, null, true);
            _blur.Dispose();
            _blur = null!;
        }
    }
}
