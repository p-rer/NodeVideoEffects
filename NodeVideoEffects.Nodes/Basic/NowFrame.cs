using NodeVideoEffects.Type;
using System.ComponentModel;
using Vortice.Direct2D1;
using Windows.Win32.UI.KeyboardAndMouseInput;

namespace NodeVideoEffects.Nodes.Basic
{
    public class NowFrame : INode
    {
        public NowFrame(ID2D1Bitmap bitmap) : base(
            [],
            [new(new Number(0, 0, null, 0), "Input")],
            "Frame")
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

        public void UpdateFPS(int fps)
        {
            Outputs[0].Value = fps;
        }

        public override async Task Calculate() { return; }
    }
}
