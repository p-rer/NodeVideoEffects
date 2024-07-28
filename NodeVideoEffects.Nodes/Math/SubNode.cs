﻿using NodeVideoEffects.Type;

namespace NodeVideoEffects.Nodes.Math
{
    public class SubNode : INode
    {
        public SubNode(string? id = null) : base(
            [
                new(new Number(0, null, null, null), "Value1"),
                new(new Number(0, null, null, null), "Value2")
            ],
            [
                new (new Number(0, null, null, null), "Result")
            ],
            "Sub", id)
        { }

        public override async Task Calculate()
        {
            this.Outputs[0].Value = (double)Inputs[0].Value - (double)Inputs[1].Value;
            return;
        }
    }
}
