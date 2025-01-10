using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NodeVideoEffects.Control
{
    /// <summary>
    /// Interaction logic for BoolPort.xaml
    /// </summary>
    public partial class BoolPort : IControl
    {
        private bool _isChecked;
        public BoolPort(bool isChecked)
        {
            InitializeComponent();
            Value = isChecked;
        }

        public object? Value
        {
            get => _isChecked;
            set
            {
                _isChecked = (bool)(value ?? true);
                Check.Fill = _isChecked ? SystemColors.HighlightBrush : SystemColors.GrayTextBrush;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void check_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Value = !_isChecked;
            e.Handled = true;
        }
    }
}
