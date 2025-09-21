using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

public class CosineNode : NodeLogic
{
    public CosineNode() : base(
        [
            new Input(new Number(0f, null, null, null, "rad"), Text_Node.Degree)
        ],
        [
            new Output(new Number(0f, null, null, null), Text_Node.Result)
        ],
        Text_Node.CosineNode,
        Colors.LightCoral,
        $"{Text_Node.MathCategory}/{Text_Node.TrigonometricCategory}")
    {
    }

    public override Task Calculate()
    {
        Outputs[0].Value = Convert.ToSingle(System.Math.Cos(Convert.ToDouble((float)(Inputs[0].Value ?? 0f))));
        return Task.CompletedTask;
    }
}