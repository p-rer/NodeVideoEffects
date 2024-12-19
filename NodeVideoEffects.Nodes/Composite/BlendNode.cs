using NodeVideoEffects.Type;
using NodeVideoEffects.Utility;
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
        Vortice.Direct2D1.Effects.Blend? blend;
        string id;

        public BlendNode(string id) : base(
            [
            new(new Type.Enum(
            [
                    "Multiply",
                    "Screen",
                    "Darken",
                    "Lighten",
                    "Dissolve",
                    "ColorBurn",
                    "LinearBurn",
                    "DarkerColor",
                    "LighterColor",
                    "ColorDodge",
                    "LinearDodge",
                    "Overlay",
                    "SoftLight",
                    "HardLight",
                    "VividLight",
                    "LinearLight",
                    "PinLight",
                    "HardMix",
                    "Difference",
                    "Exclusion",
                    "Hue",
                    "Saturation",
                    "Color",
                    "Luminosity",
                    "Subtract",
                    "Division"
                ]),
                "Mode"),
            new(new Image(null), "Input1"),
            new(new Image(null), "Input2")
            ],
            [
                new(new Image(null), "Output")
                ],
            "Blend",
            Colors.DarkViolet,
            "Composite"
            )
        {
            this.id = id;
            if (id == "")
                return;
        }

        public override async Task Calculate()
        {
            if (blend == null ||blend.NativePointer == 0)
            {
                blend = new(NodesManager.GetContext(id).DeviceContext);
            }
            if (Inputs[0].Value == null || Inputs[1].Value == null)
            {
                Logger.Write(LogLevel.Warn, $"Inputs are null. Calculation skipped.\nID: {id}\nName: {Name}");
                return;
            }
            lock (blend)
            {
                blend.SetInput(0, ((ImageAndContext)Inputs[1].Value).Image, true);
                blend.SetInput(1, ((ImageAndContext)Inputs[2].Value).Image, true);
                blend.Mode = (BlendMode)((int?)Inputs[0].Value ?? 0);
                Outputs[0].Value = new ImageAndContext(blend.Output);
            }
            return;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (blend != null)
            {
                blend?.SetInput(0, null, true);
                blend?.SetInput(1, null, true);
                blend?.Dispose();
                blend = null;
            }
        }
    }
}
