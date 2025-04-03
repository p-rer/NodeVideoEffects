using NodeVideoEffects.Core;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

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
        Text.PowNode,
        Colors.LightCoral,
        "Math/Basic")
    {
    }

    public override Task Calculate()
    {
        Outputs[0].Value = (float)System.Math.Pow((float)(Inputs[0].Value ?? 0), (float)(Inputs[1].Value ?? 0));
        return Task.CompletedTask;
    }
}