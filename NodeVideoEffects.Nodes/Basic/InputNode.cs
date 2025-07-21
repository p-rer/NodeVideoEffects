using NodeVideoEffects.Core;
using System.Windows.Media;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Nodes.Basic;

public class InputNode : NodeLogic
{
    private ID2D1Image? _image;

    public InputNode() : base(
        [],
        [new Output(new Image(null), "Input", true)],
        Text.Input,
        Colors.PaleVioletRed,
        "Basic") { }

    public ID2D1Image? Image
    {
        get => _image;
        set
        {
            if (_image == value) return;
            var connectedNode = Outputs[0].Connection;
            connectedNode.ForEach(portInfo =>
                NodesManager.GetNode(portInfo.Id)?.SetInput(portInfo.Index, new ImageWrapper(value)));
            _image = value;
        }
    }

    public override Task Calculate()
    {
        return Task.CompletedTask;
    }

    public override object GetOutput(int index) => new ImageWrapper(Image);
}