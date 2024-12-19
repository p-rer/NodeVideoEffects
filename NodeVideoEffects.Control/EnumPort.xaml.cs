using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace NodeVideoEffects.Control
{
    public partial class EnumPort : UserControl, IControl
    {
        int value = 0;
        public EnumPort(List<string> items)
        {
            InitializeComponent();

            box.ItemsSource = items;
            Value = value;
        }

        public object? Value
        {
            get => value;
            set
            {
                this.value = (int?)value ?? 0;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Value = box.SelectedIndex;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
