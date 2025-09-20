using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

public class RootNode : NodeLogic
{
    public RootNode() : base(
        [
            new Input(new Number(0f, null, null, null), Text_Node.Radicand),
            new Input(new Number(1f, null, null, null), Text_Node.Degree),
            new Input(new Bool(true), Text_Node.Allow0thRoot)
        ],
        [
            new Output(new Number(0f, null, null, null), Text_Node.Result)
        ],
        Text_Node.RootNode,
        Colors.LightCoral,
        $"{Text_Node.MathCategory}/{Text_Node.BasicCategory}")
    {
    }

    public override Task Calculate()
    {
        if ((float)(Inputs[1].Value ?? 0f) == 0f && (bool)(Inputs[2].Value ?? 0f))
            Outputs[0].Value = 0f;
        else
            Outputs[0].Value = (float)(Inputs[0].Value ?? 0f) < 0f
                ? (float)(Inputs[1].Value ?? 0f) % 2f != 0f
                    ? -System.Math.Pow(-(float)(Inputs[0].Value ?? 0f), 1f / (float)(Inputs[1].Value ?? 0f))
                    : float.NaN
                : System.Math.Pow((float)(Inputs[0].Value ?? 0f), 1f / (float)(Inputs[1].Value ?? 0f));
        return Task.CompletedTask;
    }
}