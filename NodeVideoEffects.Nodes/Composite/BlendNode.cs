using NodeVideoEffects.Type;
using NodeVideoEffects.Utility;
using System.Windows.Media;
using Vortice.Direct2D1;
using Blend = Vortice.Direct2D1.Effects.Blend;

namespace NodeVideoEffects.Nodes.Composite
{
    public class BlendNode : INode
    {
        private Blend _blend = null!;
        private readonly string _id;

        public BlendNode(string id) : base(
            [
            new Input(new Type.Enum(
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
            new Input(new Image(null), "Input1"),
            new Input(new Image(null), "Input2")
            ],
            [
                new Output(new Image(null), "Output")
                ],
            "Blend",
            Colors.DarkViolet,
            "Composite"
            )
        {
            _id = id;
            if (id == "")
                return;
            _blend = new Blend(NodesManager.GetContext(_id).DeviceContext);
        }

        public override async Task Calculate()
        {
            await Task.Run(() =>
            {
                lock (_blend)
                {
                    if (_blend.NativePointer == 0)
                    {
                        _blend = new Blend(NodesManager.GetContext(_id).DeviceContext);
                    }

                    if (Inputs[0].Value == null || Inputs[1].Value == null)
                    {
                        Logger.Write(LogLevel.Warn, $"Inputs are null. Calculation skipped.\nID: {_id}\nName: {Name}");
                        return;
                    }

                    _blend.SetInput(0, ((ImageWrapper?)Inputs[1].Value)?.Image, true);
                    _blend.SetInput(1, ((ImageWrapper?)Inputs[2].Value)?.Image, true);
                    _blend.Mode = (BlendMode)((int?)Inputs[0].Value ?? 0);
                    Outputs[0].Value = new ImageWrapper(_blend.Output);
                }
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_blend == null!)
                return;
            _blend.SetInput(0, null, true);
            _blend.SetInput(1, null, true);
            _blend.Dispose();
            _blend = null!;
        }
    }
}
