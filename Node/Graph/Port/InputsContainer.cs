using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Node.Graph.Port;

public abstract class InputsContainer
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected internal void Set<T>(ref T field, T value, [CallerMemberName] string name = null!)
    {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}