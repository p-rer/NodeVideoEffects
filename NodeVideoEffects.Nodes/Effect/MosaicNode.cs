using NodeVideoEffects.Type;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video.Effects;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects.Nodes.Effect
{
    public class MosaicNode : INode
    {
        IVideoEffect? effect;
        string id;
        public MosaicNode(string id) : base(
            [
                new(new Image(null), "In"),
                new(new Number(10, 0, 250,4), "Level")
            ],
            [
                new(new Image(null), "Out")
            ],
            "Mosaic",
            Colors.LawnGreen,
            "Effect")
        {
            if (id == "")
                return;
            effect = Activator.CreateInstance(PluginLoader.VideoEffects.ToList().Where(type => type.Name == "MosaicEffect").First()) as IVideoEffect;
            this.id = id;
        }

        public override async Task Calculate()
        {
            IGraphicsDevicesAndContext context = NodesManager.GetContext(id);
            VideoEffectProcessorBase? processor = effect?.CreateVideoEffect(context) as VideoEffectProcessorBase;

            processor?.SetInput(((ImageAndContext)Inputs[0].Value).Image);
            processor?.Update(NodesManager._EffectDescription[id]);
            Outputs[0].Value = new ImageAndContext(processor?.Output, ((ImageAndContext)Inputs[0].Value).Context);
            return;
        }
    }
}
