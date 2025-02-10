﻿using NodeVideoEffects.Type;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Math
{
    public class MulNode : NodeLogic
    {
        public MulNode() : base(
            [
                new Input(new Number(0, null, null, null), "Value1"),
                new Input(new Number(0, null, null, null), "Value2")
            ],
            [
                new Output(new Number(0, null, null, null), "Result")
            ],
            Text.MulNode,
            Colors.LightCoral,
            "Math/Basic")
        { }

        public override Task Calculate()
        {
            Outputs[0].Value = (float)(Inputs[0].Value ?? 0) * (float)(Inputs[1].Value ?? 0);
            return Task.CompletedTask;
        }
    }
}
