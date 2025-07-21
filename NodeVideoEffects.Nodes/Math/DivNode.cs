using NodeVideoEffects.Core;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

public class DivNode : NodeLogic
{
    public DivNode() : base(
        [
            new Input(new Number(0f, null, null, null), "Value1"),
            new Input(new Number(0f, null, null, null), "Value2"),
            new Input(new Bool(true), "Allow div0")
        ],
        [
            new Output(new Number(0f, null, null, null), "Result")
        ],
        Text.DivNode,
        Colors.LightCoral,
        "Math/Basic")
    {
    }

    public override Task Calculate()
    {
        if ((float)(Inputs[1].Value ?? 0f) == 0f && (bool)(Inputs[2].Value ?? true))
            Outputs[0].Value = 0f;
        else
            Outputs[0].Value = (float)(Inputs[0].Value ?? 0f) / (float)(Inputs[1].Value ?? 0f);
        return Task.CompletedTask;
    }
}