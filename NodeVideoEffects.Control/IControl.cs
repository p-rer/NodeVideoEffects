using System.ComponentModel;

namespace NodeVideoEffects.Control
{
    public interface IControl : INotifyPropertyChanged
    {
        public object? Value { get; set; }
    }
}
