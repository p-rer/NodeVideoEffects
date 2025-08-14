using NodeVideoEffects.Core;
using System.Windows.Media;
using Vortice.Direct2D1;
using Blend = Vortice.Direct2D1.Effects.Blend;
using Enum = NodeVideoEffects.Core.Enum;

namespace NodeVideoEffects.Nodes.Composite;

public class BlendNode : NodeLogic
{
    private readonly Lock _lock = new();
    private Blend? _blend;

    public BlendNode(string id) : base(
        [
            new Input(new Enum(
                [
                    "Multiply",
                    "Screen",
                    "Darken",
                    "Lighten",
                    "Dissolve",
                    "ColorBurn",
                    "LinearBurn",
                    "DarkerColor",
                    "LighterColor",
                    "ColorDodge",
                    "LinearDodge",
                    "Overlay",
                    "SoftLight",
                    "HardLight",
                    "VividLight",
                    "LinearLight",
                    "PinLight",
                    "HardMix",
                    "Difference",
                    "Exclusion",
                    "Hue",
                    "Saturation",
                    "Color",
                    "Luminosity",
                    "Subtract",
                    "Division"
                ]),
                "Mode"),
            new Input(new Image(null), "Input1"),
            new Input(new Image(null), "Input2")
        ],
        [
            new Output(new Image(null), "Output")
        ],
        "Blend",
        Colors.DarkViolet,
        "Composite"
    )
    {
        if (id == "") return;
        _blend = new Blend(NodesManager.GetContext(id).DeviceContext);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        lock (_lock)
        {
            _blend?.SetInput(0, null, true);
            _blend?.SetInput(1, null, true);
            _blend?.Dispose();
            _blend = new Blend(context);
        }
    }

    public override async Task Calculate()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_blend == null) return;
                _blend.SetInput(0, ((ImageWrapper?)Inputs[1].Value)?.Image, true);
                _blend.SetInput(1, ((ImageWrapper?)Inputs[2].Value)?.Image, true);
                _blend.Mode = (BlendMode)((int?)Inputs[0].Value ?? 0);
                Outputs[0].Value = new ImageWrapper(_blend.Output);
            }
        });
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_blend == null) return;
        _blend?.SetInput(0, null, true);
        _blend?.SetInput(1, null, true);
        _blend?.Dispose();
        _blend = null!;

        GC.SuppressFinalize(this);
    }
}