using System.Windows.Media;
using NodeVideoEffects.Core;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Nodes.Composite;

public class MaskClip : NodeLogic
{
    private readonly string _effectId = "";
    private readonly string _shaderId = "";
    private VideoEffectsLoader? _videoEffect;

    public MaskClip(string id) : base(
        [
            new Input(new Image(null), "Input"),
            new Input(new Mask(null), "Mask"),
            new Input(new Bool(false), "Invert")
        ],
        [
            new Output(new Image(null), "Output")
        ],
        "Clip with Mask",
        Colors.DarkViolet,
        "Composite")
    {
        if (id == "") return;
        _effectId = id;
        _shaderId = VideoEffectsLoader.RegisterShader("MaskClip.cso");
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(int), "Invert")
        ], _shaderId, _effectId, 2);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        _videoEffect?.Dispose();
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(int), "Invert")
        ], _shaderId, _effectId, 2);
    }

    public override Task Calculate()
    {
        if (_videoEffect == null) return Task.CompletedTask;
        _videoEffect
            .SetValue((bool?)Inputs[2].Value ?? false ? 1 : 0);
        if (_videoEffect.Update(out var output, ((ImageWrapper?)Inputs[0].Value)?.Image,
                ((MaskWrapper?)Inputs[1].Value)?.Image))
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