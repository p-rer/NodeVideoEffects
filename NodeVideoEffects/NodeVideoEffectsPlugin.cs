using System.ComponentModel.DataAnnotations;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Resources.Localization;

namespace NodeVideoEffects;

[VideoEffect("NodeVideoEffects", [VideoEffectCategories.Filtering], ["NVE", "ノード"], ResourceType = typeof(Texts),
    IsAviUtlSupported = false)]
public class NodeVideoEffectsPlugin : VideoEffectBase
{
    private bool _isCreated;

    private List<NodeInfo> _nodes = [];
    private NodeProcessor? _processor;

    internal NodeEditor? Window = null;

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

    internal List<NodeInfo> EditorNodes
    {
        set => Set(ref _nodes, value, nameof(Nodes), nameof(EditorNodes));
    }

    ~NodeVideoEffectsPlugin()
    {
        Window?.Close();
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
            _processor!.UpdateContext(devices);
            Logger.Write(LogLevel.Debug, $"Reloaded the effect processor, ID: \"{Id}\".");
            Logger.Write(LogLevel.Debug, "Nodes", Nodes);
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