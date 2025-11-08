using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Control.ViewModel;

public class FilePathPortViewModel : INotifyPropertyChanged
{
    private readonly List<(string Name, string[] Ext)> _allowExtension;
    private string _path;

    public FilePathPortViewModel(string path, List<(string Name, string[] Ext)> allowExtension)
    {
        _path = path;
        _allowExtension = allowExtension;
        OpenFileCommand = new RelayCommand(_ => OpenFileDialog());
    }

    public ICommand OpenFileCommand { get; }

    public string PathFileText => string.IsNullOrEmpty(_path)
        ? Text_Node.NoFileSelected
        : Path.GetFileName(_path);

    public string Value
    {
        get => _path;
        set
        {
            if (_path == value) return;
            _path = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PathFileText));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OpenFileDialog()
    {
        var dialog = new OpenFileDialog
        {
            DefaultExt = _allowExtension.FirstOrDefault().Ext.FirstOrDefault() ?? "*.*",
            Filter = string.Join("|",
                _allowExtension.Select(nameExtPair =>
                    $"{nameExtPair.Name}|{string.Join(";", nameExtPair.Ext.Select(ext => "*" + ext))}"))
        };

        var result = dialog.ShowDialog();
        if (result == true)
            Value = dialog.FileName;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}