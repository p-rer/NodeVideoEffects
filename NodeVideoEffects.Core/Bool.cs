﻿using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Core;

public class Bool : IPortValue
{
    private bool _value;

    /// <summary>
    /// Create new bool object
    /// </summary>
    /// <param name="value">Value</param>
    public Bool(bool value)
    {
        _value = value;
    }

    public Type Type => typeof(bool);
    public Color Color => Colors.Purple;

    /// <summary>
    /// Value
    /// </summary>
    public object Value => _value;

    public void _SetValue(object? value)
    {
        _value = (bool)(value ?? true);
    }

    public void Dispose()
    {
    }

    public IControl Control => new BoolPort(_value);
}