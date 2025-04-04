﻿using NodeVideoEffects.Core;
using System.ComponentModel;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Nodes.Basic;

public class OutputNode : NodeLogic
{
    public OutputNode() : base(
        [new Input(new Image(null), "Output")],
        [],
        Text.Output,
        Colors.PaleVioletRed,
        "Basic")
    {
        Inputs[0].PropertyChanged += (_, _) =>
        {
            if (Inputs[0].Value != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        };
    }

    public override object? GetOutput(int index)
    {
        return Inputs[0].Value;
    }

    public override Task Calculate()
    {
        return Task.CompletedTask;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}