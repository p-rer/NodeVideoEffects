using System.ComponentModel;
using NodeVideoEffects.Control.ViewModel;

namespace NodeVideoEffects.Control;

public partial class FilePathPort : IControl
{
    public FilePathPort(string path, List<(string Name, string[] Ext)> allowExtension)
    {
        InitializeComponent();
        DataContext = new FilePathPortViewModel(path, allowExtension);
    }

    public object? Value
    {
        get => ((FilePathPortViewModel)DataContext).Value;
        set => ((FilePathPortViewModel)DataContext).Value = (string)(value ?? "");
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => ((FilePathPortViewModel)DataContext).PropertyChanged += value;
        remove => ((FilePathPortViewModel)DataContext).PropertyChanged -= value;
    }
}