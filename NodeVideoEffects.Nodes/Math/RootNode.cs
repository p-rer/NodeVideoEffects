using NodeVideoEffects.Core;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

public class RootNode : NodeLogic
{
    public RootNode() : base(
        [
            new Input(new Number(0, null, null, null), "Radicand"),
            new Input(new Number(1, null, null, null), "Degree"),
            new Input(new Bool(true), "Allow 0th root")
        ],
        [
            new Output(new Number(0, null, null, null), "Result")
        ],
        Text.RootNode,
        Colors.LightCoral,
        "Math/Basic")
    {
    }

    public override Task Calculate()
    {
        if ((float)(Inputs[1].Value ?? 0) == 0 && (bool)(Inputs[2].Value ?? 0))
            Outputs[0].Value = 0;
        else
            Outputs[0].Value = (float)(Inputs[0].Value ?? 0) < 0
                ? (float)(Inputs[1].Value ?? 0) % 2 != 0
                    ? -System.Math.Pow(-(float)(Inputs[0].Value ?? 0), 1 / (float)(Inputs[1].Value ?? 0))
                    : float.NaN
                : System.Math.Pow((float)(Inputs[0].Value ?? 0), 1 / (float)(Inputs[1].Value ?? 0));
        return Task.CompletedTask;
    }
}