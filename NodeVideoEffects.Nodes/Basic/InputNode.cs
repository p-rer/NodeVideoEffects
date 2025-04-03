using NodeVideoEffects.Core;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Basic;

public class InputNode : NodeLogic
{
    public InputNode() : base(
        [],
        [new Output(new Image(null), "Input")],
        Text.Input,
        Colors.PaleVioletRed,
        "Basic")
    {
        NodesManager.InputUpdated += Input_PropertyChanged;
    }

    public override Task Calculate()
    {
        return Task.CompletedTask;
    }
}