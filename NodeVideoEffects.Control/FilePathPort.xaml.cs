using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;

namespace NodeVideoEffects.Control;

/// <summary>
///     Interaction logic for BoolPort.xaml
/// </summary>
public sealed partial class FilePathPort : IControl
{
    private readonly List<(string Name, string[] Ext)> _allowExtension;
    private string _path;

    public FilePathPort(string path, List<(string Name, string[] Ext)> allowExtension)
    {
        InitializeComponent();
        _path = path;
        if (!string.IsNullOrEmpty(path))
            Value = path;
        _allowExtension = allowExtension;
        DataContext = this;
    }

    public object PathFileText => _path == "" ? "No file selected" : Path.GetFileName(_path);

    public object? Value
    {
        get => _path;
        set
        {
            _path = (string)(value ?? "");
            OnPropertyChanged();
            OnPropertyChanged(nameof(PathFileText));
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OpenFileDialog(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            DefaultExt = _allowExtension[0].Ext[0],
            Filter = string.Join("|",
                _allowExtension.Select(nameExtPair =>
                    $"{nameExtPair.Name}|{string.Join(";", nameExtPair.Ext.Select(ext => "*" + ext))}"))
        };


        var result = dialog.ShowDialog();

        if (result == true)
            // Open document
            Value = dialog.FileName;
    }
}