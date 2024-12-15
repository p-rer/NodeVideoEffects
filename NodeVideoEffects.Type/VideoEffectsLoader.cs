using Vortice.Direct2D1;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects.Type
{
    public class VideoEffectsLoader : IDisposable
    {
        IVideoEffect videoEffect;
        IVideoEffectProcessor? processor;
        string id;

        private VideoEffectsLoader(IVideoEffect? effect, string id)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect), "Unable load effect");
            videoEffect = effect;
            this.id = id;
        }

        public bool Update(ID2D1Image image, out ID2D1Image? output)
        {
            if(processor == null)
                processor = videoEffect.CreateVideoEffect(NodesManager.GetContext(id));
            lock (processor)
            {
                try
                {
                    if(processor.Output.NativePointer == IntPtr.Zero)
                        processor = videoEffect.CreateVideoEffect(NodesManager.GetContext(id));
                    processor.SetInput(image);
                    processor.Update(NodesManager.GetInfo(id));
                    output = processor.Output;
                    return true;
                }
                catch
                {
                    output = null;
                    return false;
                }
            }
        }

        public void Dispose()
        {
            processor?.Dispose();
        }

        public static VideoEffectsLoader LoadEffect(string name, string id) =>
            new(Activator.CreateInstance(PluginLoader.VideoEffects.ToList().Where(type => type.Name == name).First()) as IVideoEffect, id);
    }
}
