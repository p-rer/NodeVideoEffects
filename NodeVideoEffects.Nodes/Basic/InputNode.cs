using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Basic
{
    public class InputNode : INode
    {
        public InputNode() : base(
            [],
            [new(new Image(null), "Input")],
            "Input",
            Colors.PaleVioletRed,
            "Basic")
        {
            NodesManager.InputUpdated += Input_PropertyChanged;
        }

        public override async Task Calculate()
        {
            return;
        }
    }
}

