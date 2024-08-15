using System.ComponentModel;

namespace NodeVideoEffects.Control
{
    public interface IControl
    {
        public object? Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
