using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Basic
{
    public class NowFrame : INode
    {
        public NowFrame(string id) : base(
            [],
            [new(new Number(0, 0, null, 0), "Input")],
            "Frame",
            Colors.IndianRed)
        {
            NodesManager.FrameChanged += FPS_PropertyChanged;
        }

        private void FPS_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Input.Value))
            {
                Outputs[0].Value = NodesManager._FPS;
            }
        }

        public override async Task Calculate() { return; }
    }
}
