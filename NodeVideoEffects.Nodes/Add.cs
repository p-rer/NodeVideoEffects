using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes
{
    public class Add : INode
    {
        public Add() : base(
            [
                new(new Number(0, null, null, null), "Value1"),
                new(new Number(0, null, null, null), "Value2")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Add")
        { }

        public override async Task Calculate()
        {
            this.Outputs[0].Value = (double)Inputs[0].Value + (double)Inputs[1].Value;
            return;
        }
    }
}
