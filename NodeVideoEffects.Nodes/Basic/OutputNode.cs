using NodeVideoEffects.Type;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Nodes.Basic
{
    public class OutputNode : INode
    {
        public OutputNode() : base(
            [new(new Image(null), "Input")],
            [],
            "Input")
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

