using NodeVideoEffects.Type;
using NodeVideoEffects.Utility;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects
{
    [VideoEffect("NodeVideoEffects", [nameof(Translate.Node)], [], ResourceType = typeof(Translate))]
    public class NodeVideoEffectsPlugin : VideoEffectBase
    {
        private bool isCreated = false;
        private NodeProcessor processor;
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
            if (isCreated){
                NodesManager.SetContext(ID, devices);
                processor._context = devices.DeviceContext;
                Logger.Write(LogLevel.Info, $"Reloaded the effect processor, ID: \"{ID}\".");
                return processor;
            }
            isCreated = true;
            return processor = new(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [];
    }
}
