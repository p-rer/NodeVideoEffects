using Vortice.Direct2D1;

namespace NodeVideoEffects.Type
{
    public struct ImageAndContext
    {
        /// <summary>
        /// Image
        /// </summary>
        public ID2D1Image? Image { get; set; }
        /// <summary>
        /// Device context
        /// </summary>
        public ID2D1DeviceContext6? Context { get; }
        public ImageAndContext(ID2D1Image? image, ID2D1DeviceContext6? context)
        {
            Image = image;
            Context = context;
        }
    }
}
