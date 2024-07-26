using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Basic
{
    public class OutputNode : INode
    {
        public OutputNode() : base(
            [new(new Bitmap(null), "Input")],
            [],
            "Input")
        {
        }

        public override async Task Calculate() { return; }
    }
}

