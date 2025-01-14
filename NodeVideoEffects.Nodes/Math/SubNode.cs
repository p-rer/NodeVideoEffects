using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Math
{
    public class SubNode : NodeLogic
    {
        public SubNode() : base(
            [
                new Input(new Number(0, null, null, null), "Value1"),
                new Input(new Number(0, null, null, null), "Value2")
            ],
            [
                new Output(new Number(0, null, null, null), "Result")
            ],
            "Sub",
            Colors.LightCoral,
            "Math/Basic")
        { }

        public override Task Calculate()
        {
            Outputs[0].Value = (double)(Inputs[0].Value ?? 0) - (double)(Inputs[1].Value ?? 0);
            return Task.CompletedTask;
        }
    }
}
