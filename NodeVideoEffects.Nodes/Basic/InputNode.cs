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

        public override async Task Calculate()
        {
            return;
        }
    }
}

