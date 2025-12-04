using System.ComponentModel;
using NodeVideoEffects.Control.ViewModel;

namespace NodeVideoEffects.Control;

public partial class EnumPort : IControl
{
    public EnumPort(List<string> items, int value)
    {
        InitializeComponent();
        DataContext = new EnumPortViewModel(items, value);
    }

    public object? Value
    {
        get => ((EnumPortViewModel)DataContext).Value;
        set => ((EnumPortViewModel)DataContext).Value = (int?)value ?? 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => ((EnumPortViewModel)DataContext).PropertyChanged += value;
        remove => ((EnumPortViewModel)DataContext).PropertyChanged -= value;
    }
}