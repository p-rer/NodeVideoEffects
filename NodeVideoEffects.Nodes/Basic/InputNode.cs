using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Basic
{
    public class InputNode : INode
    {
        private ImageAndContext? _image;

        public InputNode() : base(
            [],
            [new(new Image(null), "Input")],
            "Input")
        {
        }

        public override async Task Calculate()
        {
            return;
        }
    }
}

