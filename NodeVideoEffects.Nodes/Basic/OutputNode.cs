using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Basic;

public class OutputNode : NodeLogic
{
    public OutputNode() : base(
        [new Input(new Image(null), "Output")],
        [],
        Text_Node.Output,
        Colors.PaleVioletRed,
        "Basic")
    {
    }

    public override object? GetOutput(int index)
    {
        return Inputs[0].PortInfo.Id == ""
            ? null
            : NodesManager.GetOutputValue(Inputs[0].PortInfo.Id, Inputs[0].PortInfo.Index).GetAwaiter().GetResult();
    }

    public override Task Calculate()
    {
        return Task.CompletedTask;
    }
}