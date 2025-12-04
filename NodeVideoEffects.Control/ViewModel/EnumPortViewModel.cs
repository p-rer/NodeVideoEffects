using System.ComponentModel;

namespace NodeVideoEffects.Control.ViewModel;

public class EnumPortViewModel : INotifyPropertyChanged
{
    private int _value;

    public EnumPortViewModel(List<string> items, int value)
    {
        Items = items;
        _value = value;
    }

    public EnumPortViewModel()
    {
        Items = [];
        _value = 0;
    }

    public List<string> Items { get; }

    public int Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}