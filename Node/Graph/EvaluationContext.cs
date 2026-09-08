using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace Node.Graph;

public class EvaluationContext
{
    public EvaluationContext(IGraphicsDevicesAndContext devices, EffectDescription desc)
    {
        Devices = devices;
        EffectDescription = desc;
    }

    public IGraphicsDevicesAndContext Devices { get; }
    public EffectDescription EffectDescription { get; }
}