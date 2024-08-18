﻿using NodeVideoEffects.Type;
using System.ComponentModel;

namespace NodeVideoEffects.Nodes.Basic
{
    public class OutputNode : INode
    {
        public OutputNode() : base(
            [new(new Image(null), "Output")],
            [],
            "Output")
        {
            Inputs[0].PropertyChanged += (s, e) => 
            {
                if (Inputs[0].Value != null)
                    PropertyChanged?.Invoke(this, new(Name));
            };
        }

        public override object? GetOutput(int index)
        {
            return Inputs[0].Value;
        }

        public override async Task Calculate()
        {
            return;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

