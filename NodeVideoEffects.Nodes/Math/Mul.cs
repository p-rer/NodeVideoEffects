using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Math
{
    public class Mul : INode
    {
        public Mul() : base(
            [
                new(new Number(0, null, null, null), "Value1"),
                new(new Number(0, null, null, null), "Value2")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Mul")
        { }

        public override async Task Calculate()
        {
            this.Outputs[0].Value = (double)Inputs[0].Value * (double)Inputs[1].Value;
            return;
        }
    }
}
