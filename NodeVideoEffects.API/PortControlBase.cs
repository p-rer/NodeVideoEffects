using System.ComponentModel;
using System.Runtime.CompilerServices;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.API;

public abstract class PortControlBase<T> : System.Windows.Controls.Control, IControl
{
    private T? _value;
    public object? Value
    {
        get => _value;
        set => SetField(ref _value, (T?)value);
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<U>(ref U? field, U? value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<U>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}