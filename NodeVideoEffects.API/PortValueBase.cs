using System.Windows.Media;
using NodeVideoEffects.Control;
using NodeVideoEffects.Type;

namespace NodeVideoEffects.API;

public abstract class PortValueBase : IPortValue
{
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Release managed resources here.
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract System.Type Type { get; }
    public abstract object Value { get; }
    public abstract Color Color { get; }
    public abstract void _SetValue(object? value);
    public abstract IControl Control { get; }
}