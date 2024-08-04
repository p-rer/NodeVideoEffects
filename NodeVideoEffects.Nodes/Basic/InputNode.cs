using NodeVideoEffects.Type;

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

        public override async Task Calculate()
        {
            return;
        }
    }
}

