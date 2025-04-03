using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Core;

public class Number : IPortValue
{
    private float _value;
    private readonly float _default;
    private readonly float _min;
    private readonly float _max;
    private readonly int _digits;

    /// <summary>
    /// Create new number object
    /// </summary>
    /// <param name="default">Default number</param>
    /// <param name="min">Min value</param>
    /// <param name="max">Max value</param>
    /// <param name="digits">Number of decimal places(max:6)</param>
    public Number(float @default, float? min, float? max, int? digits)
    {
        _min = min ?? float.NaN;
        _max = max ?? float.NaN;
        _default = @default;
        _value = @default;
        var nonNullDigits = digits ?? 6;
        _digits = nonNullDigits > 6 ? 6 : nonNullDigits < 0 ? 0 : nonNullDigits;
    }

    public Type Type => typeof(float);

    public Color Color => Colors.Coral;

    /// <summary>
    /// float value
    /// </summary>
    public object Value => _value;

    public void _SetValue(object? value)
    {
        if (float.IsNaN(Convert.ToSingle(value)))
        {
            value = _default;
        }
        else
        {
            if (!float.IsNaN(_min) && Convert.ToSingle(value) < _min) value = _min;
            if (!float.IsNaN(_max) && Convert.ToSingle(value) > _max) value = _max;
        }

        _value = (float)Math.Round(Convert.ToSingle(value), _digits);
    }

    public void Dispose()
    {
    }

    public IControl Control => new NumberPort(_default, _value, _min, _max, _digits);
}