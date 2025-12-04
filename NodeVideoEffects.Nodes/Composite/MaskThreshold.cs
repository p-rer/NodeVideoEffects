using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects.Nodes.Composite;

public class MaskThreshold : NodeLogic
{
    private readonly string _effectId = "";
    private readonly string _shaderId = "";
    private VideoEffectsLoader? _videoEffect;

    public MaskThreshold(string id) : base(
        [
            new Input(new Mask(null), Text_Node.Input),
            new Input(new Number(0, 0, 100, 1, "%"), "Min"),
            new Input(new Number(100, 0, 100, 1, "%"), "Max"),
            new Input(new Bool(false), Text_Node.Invert)
        ],
        [
            new Output(new Mask(null), Text_Node.Output)
        ],
        Text_Node.MaskThresholdNode,
        Colors.DarkViolet,
        Text_Node.CompositeCategory)
    {
        if (id == "") return;
        _effectId = id;
        _shaderId = VideoEffectsLoader.RegisterShader("MaskThreshold.cso");
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(float), "Min"),
            (typeof(float), "Max"),
            (typeof(int), "Invert")
        ], _shaderId, _effectId);
    }

    public override void UpdateContext(IGraphicsDevicesAndContext context)
    {
        _videoEffect?.Dispose();
        _videoEffect = VideoEffectsLoader.LoadEffectSync([
            (typeof(float), "Min"),
            (typeof(float), "Max"),
            (typeof(int), "Invert")
        ], _shaderId, _effectId);
    }

    public override Task Calculate()
    {
        if (_videoEffect == null) return Task.CompletedTask;
        _videoEffect
            .SetValue((float?)Inputs[1].Value / 100.0f ?? 0.0f, (float?)Inputs[2].Value / 100.0f ?? 100.0f,
                (bool?)Inputs[3].Value ?? false ? 1 : 0);
        if (_videoEffect.Update(out var output, ((MaskWrapper?)Inputs[0].Value)?.Image))
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