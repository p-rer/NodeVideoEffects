using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects.Type
{
    public struct ImageAndContext
    {
        public ID2D1Image? Image { get; set; }
        public IGraphicsDevicesAndContext? Context { get; init; }
        public ImageAndContext(ID2D1Image? image, IGraphicsDevicesAndContext? context)
        {
            Image = image;
            Context = context;
        }
    }
}
