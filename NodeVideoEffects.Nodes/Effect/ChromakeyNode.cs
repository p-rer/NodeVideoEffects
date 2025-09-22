using System.Numerics;
using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;

namespace NodeVideoEffects.Nodes.Effect;

public class ChromakeyNode : NodeLogic
{
    private readonly Lock _lock = new();
    private ChromaKey? _chromakey;

    public ChromakeyNode(string id) : base(
        [
            new Input(new Image(null), Text_Node.Input),
            new Input(new ColorValue(Colors.White), Text_Node.KeyColor),
            new Input(new Number(0.1f, 0, 1, 4), Text_Node.Tolerance),
            new Input(new Bool(false), Text_Node.InvertAlpha),
            new Input(new Bool(false), Text_Node.Feather)
        ],
        [
            new Output(new Image(null), Text_Node.Output)
        ],
        Text_Node.ChromakeyNode,
        Colors.LawnGreen,
        Text_Node.EffectCategory)
    {
        if (id == "") return;
        _chromakey = new ChromaKey(NodesManager.GetContext(id).DeviceContext);
    }

    public override void UpdateContext(ID2D1DeviceContext6 context)
    {
        lock (_lock)
        {
            _chromakey?.SetInput(0, null, true);
            _chromakey?.Dispose();
            _chromakey = new ChromaKey(context);
        }
    }

    public override async Task Calculate()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_chromakey == null) return;
                var color = (Color?)Inputs[1].Value ?? Colors.White;
                _chromakey.SetInput(0, ((ImageWrapper?)Inputs[0].Value)?.Image, true);
                _chromakey.Color = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
                _chromakey.Tolerance = Convert.ToSingle(Inputs[2].Value);
                _chromakey.InvertAlpha = (bool?)Inputs[3].Value ?? false;
                _chromakey.Feather = (bool?)Inputs[4].Value ?? false;
                Outputs[0].Value = new ImageWrapper(_chromakey.Output);
            }
        });
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_chromakey == null) return;
        _chromakey?.SetInput(0, null, true);
        _chromakey?.Dispose();
        _chromakey = null;

        GC.SuppressFinalize(this);
    }
}