using System.Numerics;
using NodeVideoEffects.Core;
using System.Windows.Media;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect;

public class BrightnessNode : NodeLogic
{
    private Brightness _brightness = null!;

    public BrightnessNode(string id) : base(
        [
            new Input(new Image(null), "In"),
            new Input(new FloatVector2(1.0f, "Threshold", 1.0f, "Value", 0.0f, 1.0f), "White Point"),
            new Input(new FloatVector2(0.0f, "Threshold", 0.0f, "Value", 0.0f, 1.0f), "Black Point")
        ],
        [
            new Output(new Image(null), "Out")
        ],
        "Brightness",
        Colors.LawnGreen,
        "Effect")
    {
        if (id == "") return;
        _brightness = new Brightness(NodesManager.GetContext(id).DeviceContext);
    }

    public override async Task Calculate()
    {
        await Task.Run(() =>
        {
            lock (_brightness)
            {
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

        if (_brightness == null!) return;
        _brightness.SetInput(0, null, true);
        _brightness.Dispose();
        _brightness = null!;

        GC.SuppressFinalize(this);
    }
}