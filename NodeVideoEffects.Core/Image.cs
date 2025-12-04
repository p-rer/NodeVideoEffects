using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Newtonsoft.Json;
using NodeVideoEffects.Control;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Core;

public class Image : IPortValue
{
    private ImageWrapper _value;

    /// <summary>
    /// Create new Image object
    /// </summary>
    /// <param name="value"></param>
    public Image(ImageWrapper? value)
    {
        _value = value ?? new ImageWrapper();
    }

    public Type Type => typeof(ImageWrapper);
    public Color Color => Colors.Green;

    /// <summary>
    /// Value contains image and device context
    /// </summary>
    public object Value => _value;

    public void _SetValue(object? value)
    {
        _value = (ImageWrapper?)value ?? new ImageWrapper();
    }

    public void Dispose()
    {
        _value.Image?.Dispose();
    }

    [field: AllowNull] [field: MaybeNull] public IControl Control => field ??= new NoControlPort();
}

public struct ImageWrapper(ID2D1Image? image)
{
    /// <summary>
    /// Image
    /// </summary>
    [JsonIgnore]
    public ID2D1Image? Image { get; set; } = image;

    public override string ToString()
    {
        return "0x" + Image?.NativePointer.ToString("x8");
    }
}