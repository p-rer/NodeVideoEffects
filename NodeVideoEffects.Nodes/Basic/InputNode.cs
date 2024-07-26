﻿using NodeVideoEffects.Type;
using System.ComponentModel;
using Vortice.Direct2D1;
using Windows.Win32.Graphics.Gdi;

namespace NodeVideoEffects.Nodes.Basic
{
    public class InputNode : INode
    {
        public InputNode(ID2D1Bitmap bitmap) : base(
            [],
            [new(new Bitmap(bitmap), "Input")],
            "Input")
        {
        }

        public void UpdateImage(ID2D1Bitmap bitmap)
        {
            Outputs[0].Value = bitmap;
        }

        public override async Task Calculate() { return; }
    }
}

