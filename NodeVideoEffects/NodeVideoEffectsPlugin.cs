using NodeVideoEffects.Type;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects
{
    [VideoEffect("NodeVideoEffects", [nameof(Translate.Node)], [], ResourceType = typeof(Translate))]
    internal class NodeVideoEffectsPlugin : VideoEffectBase
    {
        ~NodeVideoEffectsPlugin()
        {
            window?.Close();
            NodesManager.RemoveItem(ID);
        }

        public override string Label => "NodeVideoEffects";

        public string ID { get; set; } = "";

        [Display(Name = nameof(Translate.NodeEditor), GroupName = nameof(Translate.NodeVideoEffects), ResourceType = typeof(Translate))]
        [OpenNodeEditor]
        public List<NodeInfo> Nodes
        {
            get => nodes; set
            {
                if (window != null)
                    window?.EditSpace.RebuildNodes(value);
                Set(ref nodes, value);
            }
        }
        private List<NodeInfo> nodes = new();

        internal NodeEditor? window = null;

        internal List<NodeInfo> EditorNodes
        {
            get => nodes; set
            {
                Set(ref nodes, value, "Nodes", "Nodes");
            }
        }

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
