﻿using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Basic
{
    public class Frame : INode
    {
        public Frame() : base(
            [],
            [new(new Number(0, 0, null, 0), "Frame")],
            "Frame",
            Colors.IndianRed,
            "Basic")
        {
            NodesManager.FrameChanged += FRAME_PropertyChanged;
            Outputs[0].Value = (double)NodesManager._FRAME;
        }

        private void FRAME_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Outputs[0].Value = (double)NodesManager._FRAME;
        }

        public override async Task Calculate() { return; }
    }
}
