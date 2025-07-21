using NodeVideoEffects.Core;
using System.Windows.Media;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect;

public class GaussianBlurNode : NodeLogic
{
    private readonly string _id;
    private GaussianBlur? _blur;
    private readonly Lock _lock = new();

    public GaussianBlurNode(string id) : base(
        [
            new Input(new Image(null), "In"),
            new Input(new Number(10, 0, 250, 4), "Level")
        ],
        [
            new Output(new Image(null), "Out")
        ],
        Text.GaussianBlurNode,
        Colors.LawnGreen,
        "Effect")
    {
        if ((_id = id) == "") return;
        _blur = new GaussianBlur(NodesManager.GetContext(id).DeviceContext);
    }

    public override async Task Calculate()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                _blur ??= new GaussianBlur(NodesManager.GetContext(_id).DeviceContext);
                _blur.SetInput(0, ((ImageWrapper?)Inputs[0].Value)?.Image, true);
                _blur.StandardDeviation = Convert.ToSingle(Inputs[1].Value);
                Outputs[0].Value = new ImageWrapper(_blur.Output);
            }
        });
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_blur == null) return;
        _blur.SetInput(0, null, true);
        _blur.Dispose();
        _blur = null;

        GC.SuppressFinalize(this);
    }
}