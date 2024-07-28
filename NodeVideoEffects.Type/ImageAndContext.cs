using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;

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
        public IGraphicsDevicesAndContext? Context { get; init; }
        public ImageAndContext(ID2D1Image? image, IGraphicsDevicesAndContext? context)
        {
            Image = image;
            Context = context;
        }
    }
}
