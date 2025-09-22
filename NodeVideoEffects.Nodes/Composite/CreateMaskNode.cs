using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
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
            new Input(new Image(null), Text_Node.Image),
            new Input(new Enum(
                [
                    Text_Node.Hue,
                    Text_Node.Saturation,
                    Text_Node.Brightness,
                    Text_Node.Red,
                    Text_Node.Green,
                    Text_Node.Blue,
                    Text_Node.Alpha
                ]),
                Text_Node.Mode),
            new Input(new Number(0.0f, 0.0f, 1.0f, 2), Text_Node.Offset),
            new Input(new Bool(false), Text_Node.Invert)
        ],
        [
            new Output(new Mask(null), Text_Node.Mask)
        ],
        Text_Node.CreateMaskNode,
        Colors.DarkViolet,
        Text_Node.CompositeCategory)
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
        if (_videoEffect.Update(out var output, ((ImageWrapper?)Inputs[0].Value)?.Image))
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