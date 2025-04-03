using System.Numerics;
using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Core;

public class FloatVector2 : IPortValue
{
    private float _value1;
    private float _value2;
    private readonly float _default1;
    private readonly string _name1;
    private readonly float _default2;
    private readonly string _name2;
    private readonly float _min;
    private readonly float _max;
    private const int Digits = 6;

    /// <summary>
    /// Create new number object
    /// </summary>
    /// <param name="default1">Default number (1)</param>
    /// <param name="name1">Name (1)</param>
    /// <param name="default2">Default number (2)</param>
    /// <param name="name2">Name (2)</param>
    /// <param name="min">Min value</param>
    /// <param name="max">Max value</param>
    public FloatVector2(float default1, string name1, float default2, string name2, float? min, float? max)
    {
        _min = min ?? float.NaN;
        _max = max ?? float.NaN;
        _default1 = default1;
        _name1 = name1;
        _default2 = default2;
        _name2 = name2;
        _value1 = default1;
        _value2 = default2;
    }

    public Type Type => typeof(List<object?>);

    public Color Color => Colors.Coral;

    /// <summary>
    /// Double value
    /// </summary>
    public object Value => new Vector2(_value1, _value2);

    public void _SetValue(object? value)
    {
        if (value is not List<object?> vector) return;
        if (float.IsNaN((float)(vector[0] ?? 0))) vector[0] = _default1;
        if (float.IsNaN((float)(vector[1] ?? 0)))
        {
            vector[1] = _default2;
        }
        else
        {
            if (!float.IsNaN(_min) && (float)(vector[0] ?? 0) < _min) vector[0] = _min;
            if (!float.IsNaN(_max) && (float)(vector[0] ?? 0) > _max) vector[0] = _max;
            if (!float.IsNaN(_min) && (float)(vector[1] ?? 0) < _min) vector[1] = _min;
            if (!float.IsNaN(_max) && (float)(vector[1] ?? 0) > _max) vector[1] = _max;
        }

        _value1 = (float)Math.Round((float)(vector[0] ?? 0), Digits);
        _value2 = (float)Math.Round((float)(vector[1] ?? 0), Digits);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IControl Control => new StackPanelPort([
        (new NumberPort(_default1, _value1, _min, _max, Digits), _name1),
        (new NumberPort(_default2, _value1, _min, _max, Digits), _name2)
    ]);
}