﻿using Vortice.Direct2D1;

namespace NodeVideoEffects.Type
{
    public struct ImageWrapper
    {
        /// <summary>
        /// Image
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public ID2D1Image? Image { get; set; }
        public ImageWrapper(ID2D1Image? image)
        {
            Image = image;
        }

        public override string? ToString() => Image?.NativePointer.ToString();
    }
}
