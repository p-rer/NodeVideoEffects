using System.Windows.Media;
using NodeVideoEffects.Type;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Nodes.Effect;

public class LensBlurNode : INode
{
    VideoEffectsLoader? videoEffect;
    private string effectId = "";
    private string shaderId = "";

    public LensBlurNode(string id) : base(
        [
            new(new Image(null), "In"),
            new(new Number(10, 0, 2000, 1), "Radius"),
            new(new Number(100, 0, 1000, 1), "Brightness"),
            new(new Number(2, 0, 10, 1), "EdgeStrength"),
            new(new Number(16, 0.5, 100, 1), "Quality")
        ],
        [
            new(new Image(null), "Out")
        ],
        "Lens Blur",
        Colors.LawnGreen,
        "Effect")
    {
        if (id == "") return;
        effectId = id;
        shaderId = VideoEffectsLoader.RegisterShader("LensBlur.cso");
    }

    public override async Task Calculate()
    {
        videoEffect ??= VideoEffectsLoader.LoadEffect([
                (typeof(float), "Radius"),
                (typeof(float), "Brightness"),
                (typeof(float), "EdgeStrength"),
                (typeof(float), "Quality")
        ], shaderId, effectId);
        videoEffect.SetValue(
            Convert.ToSingle(Inputs[1].Value),
            Convert.ToSingle(Inputs[2].Value),
            Convert.ToSingle(Inputs[3].Value),
            Convert.ToSingle(Inputs[4].Value));
        if (videoEffect.Update(((ImageAndContext)Inputs[0].Value).Image, out ID2D1Image? output))
        {
            Outputs[0].Value = new ImageAndContext(output);
        }
        return;
    }
}