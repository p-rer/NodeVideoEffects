using NodeVideoEffects.Type;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Basic
{
    public class InputNode : INode
    {
        public InputNode() : base(
            [],
            [new Output(new Image(null), "Input")],
            "Input",
            Colors.PaleVioletRed,
            "Basic")
        {
            NodesManager.InputUpdated += Input_PropertyChanged;
        }

        public override Task Calculate() => Task.CompletedTask;
    }
}

