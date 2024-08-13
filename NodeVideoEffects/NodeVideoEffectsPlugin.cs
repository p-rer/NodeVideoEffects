using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Windows.Ink;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects
{
    [VideoEffect("NodeVideoEffects", [nameof(Translate.Node)], [], ResourceType = typeof(Translate))]
    internal class NodeVideoEffectsPlugin : VideoEffectBase
    {
        public override string Label => "NodeVideoEffects";

        [Display(Name = nameof(Translate.NodeEditor), GroupName = nameof(Translate.NodeVideoEffects), ResourceType = typeof(Translate))]
        [OpenNodeEditor]
        public List<NodeInfo> Nodes { get => nodes; set => Set(ref nodes, value); }
        private List<NodeInfo> nodes = new();

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new NodeProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [];
    }
}
