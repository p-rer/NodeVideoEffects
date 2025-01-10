﻿using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows.Media;

namespace NodeVideoEffects.Nodes.Basic
{
    public class Frame : INode
    {
        private readonly string _id;
        public Frame(string id = "") : base(
            [],
            [new Output(new Number(0, 0, null, 0), "Frame")],
            "Frame",
            Colors.IndianRed,
            "Basic")
        {
            _id = id;
            NodesManager.FrameChanged += FRAME_PropertyChanged;
            var value = NodesManager.Frame.GetValueOrDefault(_id, 0);
            Outputs[0].Value = value;
        }

        private void FRAME_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var value = NodesManager.Frame.GetValueOrDefault(_id, 0);
            Outputs[0].Value = value;
        }

        public override Task Calculate() => Task.CompletedTask;
    }
}
