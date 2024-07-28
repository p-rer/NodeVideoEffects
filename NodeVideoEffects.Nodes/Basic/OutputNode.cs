using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Basic
{
    public class OutputNode : INode
    {
        public OutputNode(string? id = null) : base(
            [new(new Image(null), "Input")],
            [],
            "Input", id)
        {
        }

        public override object? GetOutput(int index)
        {
            return Inputs[0].Value;
        }

        public override async Task Calculate()
        {
            return;
        }
    }
}

