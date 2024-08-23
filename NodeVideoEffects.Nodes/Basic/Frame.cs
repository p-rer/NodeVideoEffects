using NodeVideoEffects.Type;
using System.ComponentModel;

namespace NodeVideoEffects.Nodes.Basic
{
    public class Frame : INode
    {
        public Frame() : base(
            [],
            [new(new Number(0, 0, null, 0), "Frame")],
            "Frame")
        {
            NodesManager.FrameChanged += FRAME_PropertyChanged;
        }

        private void FRAME_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Outputs[0].Value = (double)NodesManager._FRAME;
            Console.WriteLine(Outputs[0].Value);
        }

        public override async Task Calculate() { return; }
    }
}
