using System.ComponentModel;
using System.Runtime.CompilerServices;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.API;

public abstract class PortControlBase<T0> : System.Windows.Controls.Control, IControl
{
    private T0? _value;

    public object? Value
    {
        get => _value;
        set => SetField(ref _value, (T0?)value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T1>(ref T1? field, T1? value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T1>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}