using NodeVideoEffects.Core;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using Colors = System.Windows.Media.Colors;

namespace NodeVideoEffects.Nodes.Effect;

public class LensBlurNode : NodeLogic
{
    private readonly string _effectId = "";
    private readonly string _shaderId = "";
    private VideoEffectsLoader? _videoEffect;

    public LensBlurNode(string id) : base(
        [
            new Input(new Image(null), "In"),
            new Input(new Number(10, 0, 2000, 1), "Radius"),
            new Input(new Number(100, 0, 1000, 1), "Brightness"),
            new Input(new Number(2, 0, 10, 1), "EdgeStrength"),
            new Input(new Number(16, 0.5f, 100, 1), "Quality")
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
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(float), "Radius"),
            (typeof(float), "Brightness"),
            (typeof(float), "EdgeStrength"),
            (typeof(float), "Quality")
        ], _shaderId, _effectId);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        _videoEffect?.Dispose();
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(float), "Radius"),
            (typeof(float), "Brightness"),
            (typeof(float), "EdgeStrength"),
            (typeof(float), "Quality")
        ], _shaderId, _effectId);
    }

    public override Task Calculate()
    {
        if (_videoEffect == null) return Task.CompletedTask;
        var radius = Convert.ToSingle(Inputs[1].Value);
        _videoEffect
            .SetInputImageMargin(new Rect { Bottom = radius, Left = radius, Top = radius, Right = radius })
            .SetOutputImageMargin(new Rect { Bottom = radius, Left = radius, Top = radius, Right = radius })
            .SetValue(
                radius,
                Convert.ToSingle(Inputs[2].Value),
                Convert.ToSingle(Inputs[3].Value),
                Convert.ToSingle(Inputs[4].Value));
        if (_videoEffect.Update(((ImageWrapper?)Inputs[0].Value)?.Image, out var output))
            Outputs[0].Value = new ImageWrapper(output);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_videoEffect == null) return;
        _videoEffect.Dispose();
        _videoEffect = null;

        GC.SuppressFinalize(this);
    }
}