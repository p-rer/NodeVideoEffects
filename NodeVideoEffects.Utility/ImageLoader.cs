using System.Numerics;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video.Effects;
using YukkuriMovieMaker.Plugin;

namespace NodeVideoEffects.Utility;

public class ImageLoader
{
    public static ID2D1Image LoadImage(IGraphicsDevicesAndContext context, string filePath)
    {
        using var drawingEffect = new DrawingEffect(context);
        var dc = context.DeviceContext;
        var source = ImageFileSourceFactory.Create(context, filePath) ??
                     new ImageFileSource(context.DeviceContext.CreateEmptyBitmap());
        var commandList = dc.CreateCommandList();

        dc.Target = commandList;
        dc.BeginDraw();
        var size = source.Output.Size;
        dc.DrawImage(source.Output, new Vector2((int)((0f - size.Width) / 2f), (int)((0f - size.Height) / 2f)));
        dc.EndDraw();
        dc.Target = null;
        commandList.Close();
        return commandList;
    }
}