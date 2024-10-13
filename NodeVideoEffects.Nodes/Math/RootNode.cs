using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Math
{
    public class RootNode : INode
    {
        public RootNode() : base(
            [
                new(new Number(0, null, null, null), "Radicand"),
                new(new Number(1, null, null, null), "Degree"),
                new(new Bool(true),"Allow 0th root")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Root",
            Colors.LightCoral)
        { }

        public override async Task Calculate()
        {
            if ((double)Inputs[1].Value == 0 && (bool)Inputs[2].Value)
                Outputs[0].Value = 0;
            else
                this.Outputs[0].Value = (double)Inputs[0].Value < 0 ? ((double)Inputs[1].Value % 2 != 0 ? -System.Math.Pow(-(double)Inputs[0].Value, 1.0 / (double)Inputs[1].Value) : double.NaN) : System.Math.Pow((double)Inputs[0].Value, 1.0 / (double)Inputs[1].Value); ;
            return;
        }
    }
}
