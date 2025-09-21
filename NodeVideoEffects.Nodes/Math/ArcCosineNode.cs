using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math;

public class ArcCosineNode : NodeLogic
{
    public ArcCosineNode() : base(
        [
            new Input(new Number(0f, -1.0f, 1.0f, null), Text_Node.Value)
        ],
        [
            new Output(new Number(0f, null, null, null), Text_Node.Result)
        ],
        Text_Node.ArcCosineNode,
        Colors.LightCoral,
        $"{Text_Node.MathCategory}/{Text_Node.TrigonometricCategory}")
    {
    }

    public override Task Calculate()
    {
        Outputs[0].Value = Convert.ToSingle(System.Math.Acos(Convert.ToDouble((float)(Inputs[0].Value ?? 0f))));
        return Task.CompletedTask;
    }
}