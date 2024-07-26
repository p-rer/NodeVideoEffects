using NodeVideoEffects.Type;
using System.ComponentModel;
using Vortice.Direct2D1;
using Windows.Win32.Graphics.Gdi;

namespace NodeVideoEffects.Nodes.Basic
{
    public class OutputNode : INode
    {
        public OutputNode() : base(
            [new(new Bitmap(null), "Input")],
            [],
            "Input")
        {
        }

        public override async Task Calculate() { return; }
    }
}

