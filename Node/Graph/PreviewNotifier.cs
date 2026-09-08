using YukkuriMovieMaker.UndoRedo;

namespace Node.Graph;

public sealed class PreviewNotifier : UndoRedoable
{
    private int _tick;

    public int Tick
    {
        get => _tick;
        set => _tick = value;
    }

    public void Notify()
    {
        Set(ref _tick, unchecked(_tick + 1), nameof(Tick));
    }
}