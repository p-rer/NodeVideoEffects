using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Math
{
    public class PowNode : NodeLogic
    {
        public PowNode() : base(
            [
                new Input(new Number(0, null, null, null), "Base"),
                new Input(new Number(0, null, null, null), "Exponent")
            ],
            [
                new Output(new Number(0, null, null, null), "Result")
            ],
            "Pow",
            Colors.LightCoral,
            "Math/Basic")
        { }

        public override Task Calculate()
        {
            Outputs[0].Value = System.Math.Pow((double)(Inputs[0].Value ?? 0), (double)(Inputs[1].Value ?? 0));
            return Task.CompletedTask;
        }
    }
}
