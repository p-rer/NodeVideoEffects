using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Core;

public class ColorValue(Color? color) : IPortValue
{
    private Color _value = color ?? Colors.White;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Type Type => typeof(Color);
    public object Value => _value;
    public Color Color => Colors.Olive;

    public void _SetValue(object? value)
    {
        _value = (Color?)value ?? Colors.White;
    }

    [field: AllowNull] [field: MaybeNull] public IControl Control => field ??= new ColorPort(_value);
}