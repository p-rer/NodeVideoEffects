using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NodeVideoEffects.Control
{
    /// <summary>
    /// Interaction logic for BoolPort.xaml
    /// </summary>
    public partial class BoolPort : UserControl, IControl
    {
        private bool isChecked;
        public BoolPort(bool isChecked)
        {
            InitializeComponent();
            Value = isChecked;
        }

        public object? Value
        {
            get => isChecked;
            set
            {
                isChecked = (bool)(value ?? true);
                check.Fill = isChecked ? SystemColors.HighlightBrush : SystemColors.GrayTextBrush;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void check_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Value = !isChecked;
            e.Handled = true;
        }
    }
}
