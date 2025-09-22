using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

public class DivNode : NodeLogic
{
    public DivNode() : base(
        [
            new Input(new Number(0f, null, null, null), Text_Node.Value1),
            new Input(new Number(0f, null, null, null), Text_Node.Value2),
            new Input(new Bool(true), Text_Node.AllowDiv0)
        ],
        [
            new Output(new Number(0f, null, null, null), Text_Node.Result)
        ],
        Text_Node.DivNode,
        Colors.LightCoral,
        $"{Text_Node.MathCategory}/{Text_Node.BasicCategory}")
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