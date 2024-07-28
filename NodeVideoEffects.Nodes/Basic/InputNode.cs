using NodeVideoEffects.Type;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects.Nodes.Basic
{
    public class InputNode : INode
    {
        private ImageAndContext? _image;

        public InputNode(string? id = null) : base(
            [],
            [new(new Image(null), "Input")],
            "Input", id)
        {
        }

        public override object? GetOutput(int index)
        {
            return _image;
        }

        public void SetImage(ID2D1Image? image, IGraphicsDevicesAndContext? context)
        {
            _image = new(image, context);
        }

        public override async Task Calculate()
        {
            return;
        }
    }
}

