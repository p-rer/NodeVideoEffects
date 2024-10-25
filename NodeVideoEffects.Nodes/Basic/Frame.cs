using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Basic
{
    public class Frame : INode
    {
        string _id;
        public Frame(string id = "") : base(
            [],
            [new(new Number(0, 0, null, 0), "Frame")],
            "Frame",
            Colors.IndianRed,
            "Basic")
        {
            _id = id;
            NodesManager.FrameChanged += FRAME_PropertyChanged;
            int value;
            if (!NodesManager._FRAME.TryGetValue(_id, out value))
                value = 0;
            Outputs[0].Value = value;
        }

        private void FRAME_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            int value;
            if (!NodesManager._FRAME.TryGetValue(_id, out value))
                value = 0;
            Outputs[0].Value = value;
        }

        public override async Task Calculate() { return; }
    }
}
