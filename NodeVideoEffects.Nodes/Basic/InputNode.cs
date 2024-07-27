using NodeVideoEffects.Type;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Nodes.Basic
{
    public class InputNode : INode
    {
        private ID2D1Image _image;

        public InputNode() : base(
            [],
            [new(new Image(null), "Input")],
            "Input")
        {
        }

        public override object? GetOutput(int index)
        {
            return _image;
        }

        public void SetImage(ID2D1Image image)
        {
            _image = image;
        }

        public override async Task Calculate()
        {
            return;
        }
    }
}

