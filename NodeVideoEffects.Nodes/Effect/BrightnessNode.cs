using System.Numerics;
using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect;

public class BrightnessNode : NodeLogic
{
    private readonly Lock _lock = new();
    private Brightness? _brightness;

    public BrightnessNode(string id) : base(
        [
            new Input(new Image(null), Text_Node.Input),
            new Input(new FloatVector2(1.0f, Text_Node.Threshold, 1.0f, Text_Node.Value, 0.0f, 1.0f),
                Text_Node.WhitePoint),
            new Input(new FloatVector2(0.0f, Text_Node.Threshold, 0.0f, Text_Node.Value, 0.0f, 1.0f),
                Text_Node.BlackPoint)
        ],
        [
            new Output(new Image(null), Text_Node.Output)
        ],
        Text_Node.Brightness,
        Colors.LawnGreen,
        Text_Node.EffectCategory)
    {
        if (id == "") return;
        _brightness = new Brightness(NodesManager.GetContext(id).DeviceContext);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        lock (_lock)
        {
            _brightness?.SetInput(0, null, true);
            _brightness?.Dispose();
            _brightness = new Brightness(context);
        }
    }

    public override async Task Calculate()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_brightness == null) return;
                _brightness.SetInput(0, ((ImageWrapper?)Inputs[0].Value)?.Image, true);
                _brightness.WhitePoint = Inputs[1].Value as Vector2? ?? new Vector2(1, 1);
                _brightness.BlackPoint = Inputs[2].Value as Vector2? ?? new Vector2(0, 0);
                Outputs[0].Value = new ImageWrapper(_brightness.Output);
            }
        });
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_brightness == null) return;
        _brightness?.SetInput(0, null, true);
        _brightness?.Dispose();
        _brightness = null;

        GC.SuppressFinalize(this);
    }
}