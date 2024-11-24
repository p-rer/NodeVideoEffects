using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Player.Video;
using NodeVideoEffects.Type;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Utility
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

        public ID2D1Image Update(ID2D1Image image)
        {
            processor?.Dispose();
            processor = videoEffect.CreateVideoEffect(NodesManager.GetContext(id));
            processor.SetInput(image);
            processor.Update(NodesManager.GetInfo(id));
            return processor.Output;
        }

        public void Dispose() => processor?.Dispose();

        public static VideoEffectsLoader LoadEffect(string name, string id) =>
            new(Activator.CreateInstance(PluginLoader.VideoEffects.ToList().Where(type => type.Name == name).First()) as IVideoEffect, id);        
    }
}
