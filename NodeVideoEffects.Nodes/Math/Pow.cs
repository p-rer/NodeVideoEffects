using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Math
{
    public class Pow : INode
    {
        public Pow() : base(
            [
                new(new Number(0, null, null, null), "Base"),
                new(new Number(0, null, null, null), "Exponent")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Pow")
        { }

        public override async Task Calculate()
        {
            this.Outputs[0].Value = System.Math.Pow((double)Inputs[0].Value, (double)Inputs[1].Value);
            return;
        }
    }
}
