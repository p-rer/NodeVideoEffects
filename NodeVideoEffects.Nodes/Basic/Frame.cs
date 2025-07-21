using NodeVideoEffects.Core;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Basic;

public class Frame : NodeLogic
{
    private readonly string _id;

    public Frame(string id = "") : base(
        [],
        [new Output(new Number(0, 0, null, 0), "Frame")],
        Text.FrameNode,
        Colors.IndianRed,
        "Basic")
    {
        _id = id;
        NodesManager.FrameChanged += Input_PropertyChanged;
    }

    public override Task Calculate()
    {
        return Task.CompletedTask;
    }
}