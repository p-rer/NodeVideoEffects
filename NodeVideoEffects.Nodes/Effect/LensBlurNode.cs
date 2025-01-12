using System.Windows.Media;
using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Effect;

public class LensBlurNode : INode
{
    private VideoEffectsLoader? _videoEffect;
    private readonly string _effectId = "";
    private readonly string _shaderId = "";

    public LensBlurNode(string id) : base(
        [
            new Input(new Image(null), "In"),
            new Input(new Number(10, 0, 2000, 1), "Radius"),
            new Input(new Number(100, 0, 1000, 1), "Brightness"),
            new Input(new Number(2, 0, 10, 1), "EdgeStrength"),
            new Input(new Number(16, 0.5, 100, 1), "Quality")
        ],
        [
            new Output(new Image(null), "Out")
        ],
        "Lens Blur",
        Colors.LawnGreen,
        "Effect")
    {
        if (id == "") return;
        _effectId = id;
        _shaderId = VideoEffectsLoader.RegisterShader("LensBlur.cso");
    }

    public override async Task Calculate()
    {
        _videoEffect ??= await VideoEffectsLoader.LoadEffect([
                (typeof(float), "Radius"),
                (typeof(float), "Brightness"),
                (typeof(float), "EdgeStrength"),
                (typeof(float), "Quality")
        ], _shaderId, _effectId);
        _videoEffect.SetValue(
            Convert.ToSingle(Inputs[1].Value),
            Convert.ToSingle(Inputs[2].Value),
            Convert.ToSingle(Inputs[3].Value),
            Convert.ToSingle(Inputs[4].Value));
        if (_videoEffect.Update(((ImageWrapper?)Inputs[0].Value)?.Image, out var output))
        {
            Outputs[0].Value = new ImageWrapper(output);
        }
    }
}