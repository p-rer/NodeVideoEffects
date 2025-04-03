using System.ComponentModel;
using System.Windows.Controls;

namespace NodeVideoEffects.Control;

public sealed partial class EnumPort : IControl
{
    private int _value;

    public EnumPort(List<string> items, int value)
    {
        InitializeComponent();

        Box.ItemsSource = items;
        Box.SelectedIndex = value;
        Value = value;
    }

    public object? Value
    {
        get => _value;
        set
        {
            _value = (int?)value ?? 0;
            OnPropertyChanged(nameof(Value));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Value = Box.SelectedIndex;
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}