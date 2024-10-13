using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Math
{
    public class PowNode : INode
    {
        public PowNode() : base(
            [
                new(new Number(0, null, null, null), "Base"),
                new(new Number(0, null, null, null), "Exponent")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Pow",
            Colors.LightCoral)
        { }

        public override async Task Calculate()
        {
            this.Outputs[0].Value = System.Math.Pow((double)Inputs[0].Value, (double)Inputs[1].Value);
            return;
        }
    }
}
