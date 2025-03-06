using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Type;

public class ColorValue : IPortValue
{
    private Color _value;

    public ColorValue(Color? color)
    {
        _value = color ?? Colors.White;
        Control = new ColorPort(_value);
    }

    public void Dispose() => GC.SuppressFinalize(this);

    public System.Type Type => typeof(Color);
    public object Value => _value;
    public Color Color => Colors.Olive;
    public void _SetValue(object? value)
    {
        _value = (Color?)value ?? Colors.White;
    }

    public IControl Control { get; }
}