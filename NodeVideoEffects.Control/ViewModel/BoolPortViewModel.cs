using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Control.ViewModel;

public class BoolPortViewModel : INotifyPropertyChanged
{
    private Brush _brush = SystemColors.GrayTextBrush;
    private bool _isChecked;

    public BoolPortViewModel()
    {
        ToggleCommand = new RelayCommand(_ => Toggle());
        UpdateBrush();
    }

    public ICommand ToggleCommand { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            _isChecked = value;
            OnPropertyChanged();
            UpdateBrush();
        }
    }

    public Brush Brush
    {
        get => _brush;
        private set
        {
            if (_brush == value) return;
            _brush = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Toggle()
    {
        IsChecked = !IsChecked;
    }

    private void UpdateBrush()
    {
        Brush = _isChecked ? SystemColors.HighlightBrush : SystemColors.GrayTextBrush;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}