using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Composite;

public class MaskToImageNode : NodeLogic
{
    public MaskToImageNode() : base(
        [
            new Input(new Mask(null), Text_Node.Mask)
        ],
        [
            new Output(new Image(null), Text_Node.Image)
        ],
        Text_Node.MaskToImageNode,
        Colors.DarkViolet,
        Text_Node.CompositeCategory)
    {
    }

    public override Task Calculate()
    {
        Outputs[0].Value = new ImageWrapper(((MaskWrapper?)Inputs[0].Value)?.Image);
        return Task.CompletedTask;
    }
}