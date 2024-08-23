using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public object? Value { get => isChecked;
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
            Value = isChecked ? false : true;
            e.Handled = true;
        }
    }
}
