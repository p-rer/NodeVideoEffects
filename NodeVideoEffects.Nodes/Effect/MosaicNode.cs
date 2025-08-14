using System.Windows.Media;
using NodeVideoEffects.Core;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Project.Effects;
using Enum = NodeVideoEffects.Core.Enum;

namespace NodeVideoEffects.Nodes.Effect;

public class MosaicNode : NodeLogic
{
    private readonly string _effectId = "";
    private VideoEffectsLoader? _videoEffect;

    public MosaicNode(string id) : base(
        [
            new Input(new Image(null), "In"),
            new Input(new Enum(["Circle", "Triangle", "Rectangle", "Hexagon", "Delaunay", "Voronoi"], 2),
                "Mosaic Type"),
            new Input(new Number(10, 1, 250, 4), "Level")
        ],
        [
            new Output(new Image(null), "Out")
        ],
        "Mosaic",
        Colors.LawnGreen,
        "Effect")
    {
        if (id == "")
            return;
        _videoEffect = VideoEffectsLoader.LoadEffectSync("MosaicEffect", _effectId = id);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        _videoEffect?.Dispose();
        _videoEffect = VideoEffectsLoader.LoadEffectSync("MosaicEffect", _effectId);
    }

    public override async Task Calculate()
    {
        if (_videoEffect == null) return;
        await (await _videoEffect.SetValue("MosaicType", (MosaicType)((int?)Inputs[1].Value ?? 2)))
            .SetValue("Size", (float?)Inputs[2].Value ?? 10f);
        if (_videoEffect.Update(((ImageWrapper?)Inputs[0].Value)?.Image, out var output))
            Outputs[0].Value = new ImageWrapper(output);
    }

    public override void Dispose()
    {
        base.Dispose();
        _videoEffect?.Dispose();
        _videoEffect = null!;

        GC.SuppressFinalize(this);
    }
}