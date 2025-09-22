using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect;

public class GaussianBlurNode : NodeLogic
{
    private readonly Lock _lock = new();
    private GaussianBlur? _blur;

    public GaussianBlurNode(string id) : base(
        [
            new Input(new Image(null), Text_Node.Input),
            new Input(new Number(10, 0, 250, 4), Text_Node.Level)
        ],
        [
            new Output(new Image(null), Text_Node.Output)
        ],
        Text_Node.GaussianBlurNode,
        Colors.LawnGreen,
        Text_Node.EffectCategory)
    {
        if (id == "") return;
        _blur = new GaussianBlur(NodesManager.GetContext(id).DeviceContext);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        lock (_lock)
        {
            _blur?.SetInput(0, null, true);
            _blur?.Dispose();
            _blur = new GaussianBlur(context);
        }
    }

    public override async Task Calculate()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_blur == null) return;
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