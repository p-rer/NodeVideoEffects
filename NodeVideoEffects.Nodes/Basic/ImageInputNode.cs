using System.Windows.Media;
using NodeVideoEffects.Core;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects.Nodes.Basic;

public class ImageInputNode : NodeLogic
{
    private readonly Lock _lock = new();
    private IGraphicsDevicesAndContext? _context;

    public ImageInputNode(string id) : base(
        [
            new Input(new FilePath("", [
                (Text_Node.Image, [
                    ".png", ".jpg", ".jpeg", ".jpe", ".jfif", ".bmp", ".dib", ".gif", ".ico", ".tiff",
                    ".tif", ".hdp", ".dds", ".dng", ".heic", ".heif", ".hif", ".avif", ".jxr", ".wdp",
                    ".webp", ".psd", ".psb", ".svg"
                ])
            ]), Text_Node.File)
        ],
        [new Output(new Image(null), Text_Node.Image)],
        Text_Node.ImageInputNode,
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
            _context = context;
        }
    }

    public override Task Calculate()
    {
        Outputs[0].Value = new ImageWrapper(ImageLoader.LoadImage(_context!, (string)Inputs[0].Value!));
        return Task.CompletedTask;
    }
}