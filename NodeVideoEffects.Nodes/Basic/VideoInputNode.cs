using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects.Nodes.Basic;

public class VideoInputNode : NodeLogic
{
    private readonly Lock _lock = new();
    private IGraphicsDevicesAndContext? _context;
    private string _lastPath = "";
    private ImageLoader.VideoLoader? _videoLoader;

    public VideoInputNode(string id) : base(
        [
            new Input(new FilePath("", [
                (Text_Node.Video, [
                    "mp4", "avi", "wmv", "3g2", "3gp", "3gp2", "3gpp", "m4v", "mov", "qt",
                    "m2ts", "m2t", "mts", "ts", "mkv", "mpg", "mpeg", "webm", "gif", "webp"
                ])
            ]), Text_Node.File),
            new Input(new Number(0, 0, null, 0), Text_Node.Frame)
        ],
        [new Output(new Image(null), Text_Node.Input)],
        Text_Node.VideoInputNode,
        Colors.PaleVioletRed,
        Text_Node.BasicCategory)
    {
        if (string.IsNullOrEmpty(id))
            return;
        _context = NodesManager.GetContext(id);
    }

    public override void UpdateContext(IGraphicsDevicesAndContext context)
    {
        lock (_lock)
        {
            _videoLoader?.Dispose();
            _videoLoader = null;
            _context = context;
        }
    }

    public override Task Calculate()
    {
        lock (_lock)
        {
            if (_context == null) return Task.CompletedTask;
            if (_videoLoader == null || _lastPath != (string)Inputs[0].Value!)
            {
                _lastPath = (string)Inputs[0].Value!;
                if (_lastPath == "") return Task.CompletedTask;
                _videoLoader = ImageLoader.CreateVideoLoader(_context, _lastPath);
                ((Number)Inputs[1].PortValue).ChengePortSetting(null, _videoLoader!.Length, null, null);
            }

            Outputs[0].Value = new ImageWrapper(_videoLoader!.LoadImage(Convert.ToInt32((float)Inputs[1].Value!)));
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        base.Dispose();

        _videoLoader?.Dispose();
        _videoLoader = null;

        GC.SuppressFinalize(this);
    }
}