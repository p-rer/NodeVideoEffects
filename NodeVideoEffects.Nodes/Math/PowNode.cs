using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Math
{
    public class PowNode : INode
    {
        public PowNode(string? id = null) : base(
            [
                new(new Number(0, null, null, null), "Base"),
                new(new Number(0, null, null, null), "Exponent")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Pow", id)
        { }

        public override async Task Calculate()
        {
            this.Outputs[0].Value = System.Math.Pow((double)Inputs[0].Value, (double)Inputs[1].Value);
            return;
        }
    }
}
