using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Basic
{
    public class InputNode : INode
    {
        private ImageAndContext? _image;

        public InputNode() : base(
            [],
            [new(new Image(null), "Input")],
            "Input",
            Colors.PaleVioletRed,
            "Basic")
        {
        }

        public override async Task Calculate()
        {
            return;
        }
    }
}

