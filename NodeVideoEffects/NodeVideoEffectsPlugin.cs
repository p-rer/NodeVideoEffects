using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects;

[VideoEffect("NodeVideoEffects", [VideoEffectCategories.Filtering], ["NVE", "ノード"])]
public class NodeVideoEffectsPlugin : VideoEffectBase
{
    private bool _isCreated;
    private NodeProcessor? _processor;

    ~NodeVideoEffectsPlugin()
    {
        Window?.Close();
        NodesManager.RemoveItem(Id);
    }

    public override string Label => "NodeVideoEffects";

    public string Id { get; set; } = "";

    [Display(Name = nameof(Text.NodeEditor), GroupName = nameof(Text.NodeVideoEffects), ResourceType = typeof(Text))]
    [OpenNodeEditor]
    public List<NodeInfo> Nodes
    {
        get => _nodes;
        set
        {
            Window?.EditSpace.RebuildNodes(value);
            _nodes = value;
        }
    }

    private List<NodeInfo> _nodes = [];

    internal NodeEditor? Window = null;

    internal List<NodeInfo> EditorNodes
    {
        set => Set(ref _nodes, value, nameof(Nodes), nameof(EditorNodes));
    }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex,
        ExoOutputDescription exoOutputDescription)
    {
        return [];
    }

    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
    {
        if (_isCreated)
        {
            NodesManager.SetContext(Id, devices);
            _processor!.Context = devices.DeviceContext;
#if DEBUG
            Logger.Write(LogLevel.Debug, $"Reloaded the effect processor, ID: \"{Id}\".");
            Logger.Write(LogLevel.Debug, "Nodes", Nodes);
#endif // Debug
            return _processor;
        }

        _isCreated = true;
        return _processor = new NodeProcessor(devices, this);
    }

    protected override IEnumerable<IAnimatable> GetAnimatables()
    {
        return [];
    }
}