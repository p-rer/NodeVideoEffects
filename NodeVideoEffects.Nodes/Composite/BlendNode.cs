using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Resources.Localization;
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
                    Texts.BlendMultiplyName,
                    Texts.BlendScreenName,
                    Texts.BlendDarkerName,
                    Texts.BlendLighterName,
                    Texts.BlendDissolveName,
                    Texts.BlendColorBurnName,
                    Texts.BlendLinearBurnName,
                    Texts.BlendDarkerColorName,
                    Texts.BlendLighterColorName,
                    Texts.BlendColorDodgeName,
                    Texts.BlendLinearDodgeName,
                    Texts.BlendOverlayName,
                    Texts.BlendSoftLightName,
                    Texts.BlendHardLightName,
                    Texts.BlendVividLightName,
                    Texts.BlendLinearLightName,
                    Texts.BlendPinLightName,
                    Texts.BlendHardMixName,
                    Texts.BlendDifferenceName,
                    Texts.BlendExclusionName,
                    Texts.BlendHueName,
                    Texts.BlendSaturationName,
                    Texts.BlendColorName,
                    Texts.BlendLuminosityName,
                    Texts.BlendSubtractName,
                    Texts.BlendDivisionName
                ]),
                Text_Node.Mode),
            new Input(new Image(null), Text_Node.Input1),
            new Input(new Image(null), Text_Node.Input2)
        ],
        [
            new Output(new Image(null), Text_Node.Output)
        ],
        Text_Node.BlendNode,
        Colors.DarkViolet,
        Text_Node.CompositeCategory
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