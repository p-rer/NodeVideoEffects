using System.ComponentModel;
using NodeVideoEffects.Control.ViewModel;

namespace NodeVideoEffects.Control;

public partial class BoolPort : IControl
{
    public BoolPort(bool isChecked)
    {
        InitializeComponent();
        if (DataContext is BoolPortViewModel vm)
            vm.IsChecked = isChecked;
    }

    public object? Value
    {
        get => (DataContext as BoolPortViewModel)?.IsChecked;
        set
        {
            if (DataContext is BoolPortViewModel vm)
                vm.IsChecked = (bool)(value ?? true);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => (DataContext as BoolPortViewModel)!.PropertyChanged += value;
        remove => (DataContext as BoolPortViewModel)!.PropertyChanged -= value;
    }
}