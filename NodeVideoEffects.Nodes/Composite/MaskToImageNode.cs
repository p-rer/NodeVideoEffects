using System.Windows.Media;
using NodeVideoEffects.Core;

namespace NodeVideoEffects.Nodes.Composite;

public class MaskToImageNode : NodeLogic
{
    public MaskToImageNode() : base(
        [
            new Input(new Mask(null), "Mask")
        ],
        [
            new Output(new Image(null), "Image")
        ],
        "Mask To Image",
        Colors.DarkViolet,
        "Composite")
    {
    }

    public override Task Calculate()
    {
        Outputs[0].Value = new ImageWrapper(((MaskWrapper?)Inputs[0].Value)?.Image);
        return Task.CompletedTask;
    }
}