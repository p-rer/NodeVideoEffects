using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Newtonsoft.Json;
using NodeVideoEffects.Control;
using Vortice.Direct2D1;

namespace NodeVideoEffects.Core;

public class Mask : IPortValue
{
    private MaskWrapper _value;

    /// <summary>
    ///     Create new Mask Image object
    /// </summary>
    /// <param name="value"></param>
    public Mask(MaskWrapper? value)
    {
        _value = value ?? new MaskWrapper();
    }

    public Type Type => typeof(MaskWrapper);
    public Color Color => Colors.Green;

    /// <summary>
    ///     Value contains image and device context
    /// </summary>
    public object Value => _value;

    public void _SetValue(object? value)
    {
        _value = (MaskWrapper?)value ?? new MaskWrapper();
    }

    public void Dispose()
    {
        _value.Image?.Dispose();
    }

    [field: AllowNull] [field: MaybeNull] public IControl Control => field ??= new NoControlPort();
}

public struct MaskWrapper(ID2D1Image? image)
{
    /// <summary>
    ///     Mask Image
    /// </summary>
    [JsonIgnore]
    public ID2D1Image? Image { get; set; } = image;

    public override string ToString()
    {
        return "0x" + Image?.NativePointer.ToString("x8");
    }
}