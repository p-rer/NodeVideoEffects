using NodeVideoEffects.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Nodes.Composite
{
    public class BlendNode : INode
    {
        Vortice.Direct2D1.Effects.Blend blend;

        public BlendNode(string id) : base(
            [
            new(new Image(null), "Input1"),
            new(new Image(null), "Input2"),
            ],
            [
                new(new Image(null), "Output")
                ],
            "Blend",
            Colors.DarkViolet,
            "Composite"
            )
        {
            if(id != "")
                blend = new(NodesManager.GetContext(id).DeviceContext);
        }

        public override async Task Calculate()
        {
            blend.SetInput(0, ((ImageAndContext)Inputs[0].Value).Image, true);
            blend.SetInput(1, ((ImageAndContext)Inputs[1].Value).Image, true);
            blend.Mode = BlendMode.Multiply;
            Outputs[0].Value = new ImageAndContext(blend.Output);
            return;
        }

        public override void Dispose()
        {
            base.Dispose();
            blend?.SetInput(0, null, true);
            blend?.SetInput(1, null, true);
            blend?.Dispose();
        }
    }
}
