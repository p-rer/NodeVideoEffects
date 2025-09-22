using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

public class MulNode : NodeLogic
{
    public MulNode() : base(
        [
            new Input(new Number(0f, null, null, null), Text_Node.Value1),
            new Input(new Number(0f, null, null, null), Text_Node.Value2)
        ],
        [
            new Output(new Number(0f, null, null, null), Text_Node.Result)
        ],
        Text_Node.MulNode,
        Colors.LightCoral,
        $"{Text_Node.MathCategory}/{Text_Node.BasicCategory}")
    {
    }

    public override Task Calculate()
    {
        Outputs[0].Value = (float)(Inputs[0].Value ?? 0f) * (float)(Inputs[1].Value ?? 0f);
        return Task.CompletedTask;
    }
}