using NodeVideoEffects.Core;
using Vortice.Direct2D1;
using Colors = System.Windows.Media.Colors;
using Enum = NodeVideoEffects.Core.Enum;

namespace NodeVideoEffects.Nodes.Composite;

public class CreateMaskNode : NodeLogic
{
    private readonly string _effectId = "";
    private readonly string _shaderId = "";
    private VideoEffectsLoader? _videoEffect;

    public CreateMaskNode(string id) : base(
        [
            new Input(new Image(null), "Image"),
            new Input(new Enum(
                [
                    "Hue",
                    "Saturation",
                    "Brightness",
                    "Red",
                    "Green",
                    "Blue",
                    "Alpha"
                ]),
                "Mode"),
            new Input(new Number(0.0f, 0.0f, 1.0f, 2), "Offset"),
            new Input(new Bool(false), "Invert")
        ],
        [
            new Output(new Mask(null), "Mask")
        ],
        "Create Mask",
        Colors.DarkViolet,
        "Composite")
    {
        if (id == "") return;
        _effectId = id;
        _shaderId = VideoEffectsLoader.RegisterShader("CreateMask.cso");
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(int), "Mode"),
            (typeof(float), "Offset"),
            (typeof(int), "Invert")
        ], _shaderId, _effectId);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        _videoEffect?.Dispose();
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(int), "Mode"),
            (typeof(float), "Offset"),
            (typeof(int), "Invert")
        ], _shaderId, _effectId);
    }

    public override Task Calculate()
    {
        if (_videoEffect == null) return Task.CompletedTask;
        _videoEffect
            .SetValue(
                Convert.ToInt32(Inputs[1].Value),
                Convert.ToSingle(Inputs[2].Value),
                (bool?)Inputs[3].Value ?? false ? 1 : 0);
        if (_videoEffect.Update(((ImageWrapper?)Inputs[0].Value)?.Image, out var output))
            Outputs[0].Value = new MaskWrapper(output);
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