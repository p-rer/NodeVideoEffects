using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Core;

public class Enum(List<string> items, int value = 0) : IPortValue
{
    private int _value = value;

    public Type Type => typeof(int);

    public object Value => _value;

    public Color Color => Colors.CornflowerBlue;

    [field: AllowNull] [field: MaybeNull] public IControl Control => field ??= new EnumPort(items, _value);

    public void Dispose()
    {
    }

    public void _SetValue(object? value)
    {
        _value = (int?)value ?? 0;
    }
}