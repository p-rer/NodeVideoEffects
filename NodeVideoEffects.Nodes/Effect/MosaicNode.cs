using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Effect
{
    public class MosaicNode : NodeLogic
    {
        private VideoEffectsLoader? _videoEffect;
        private readonly string _effectId = "";
        public MosaicNode(string id) : base(
            [
                new Input(new Image(null), "In"),
                new Input(new Number(10, 0, 250,4), "Level")
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
            _effectId = id;
        }

        public override async Task Calculate()
        {
            _videoEffect ??= await VideoEffectsLoader.LoadEffect("MosaicEffect", _effectId);
            if (_videoEffect.Update(((ImageWrapper?)Inputs[0].Value)?.Image, out var output))
            {
                Outputs[0].Value = new ImageWrapper(output);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _videoEffect?.Dispose();
        }
    }
}