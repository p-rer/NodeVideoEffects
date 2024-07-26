using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Math
{
    public class DivNode : INode
    {
        public DivNode() : base(
            [
                new(new Number(0, null, null, null), "Value1"),
                new(new Number(0, null, null, null), "Value2"),
                new(new Bool(true),"Allow div0")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Mul")
        { }

        public override async Task Calculate()
        {
            if ((double)Inputs[1].Value == 0 && (bool)Inputs[2].Value)
                this.Outputs[0].Value = 0.0;
            else
                this.Outputs[0].Value = (double)Inputs[0].Value / (double)Inputs[1].Value;
            return;
        }
    }
}
