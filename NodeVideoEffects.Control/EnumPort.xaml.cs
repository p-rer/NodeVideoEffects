using System.ComponentModel;
using System.Windows.Controls;

namespace NodeVideoEffects.Control
{
    public partial class EnumPort : IControl
    {
        private int _value;
        public EnumPort(List<string> items)
        {
            InitializeComponent();

            Box.ItemsSource = items;
            Value = _value;
        }

        public object? Value
        {
            get => _value;
            set
            {
                _value = (int?)value ?? 0;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Value = Box.SelectedIndex;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
