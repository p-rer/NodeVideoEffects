using System.ComponentModel;

namespace NodeVideoEffects.Control;

public partial class NoControlPort : IControl
{
    private object? _value;
    public NoControlPort()
    {
        Dispatcher.BeginInvoke(() => InitializeComponent);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public object? Value
    {
        get => _value;
        set
        {
            _value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}