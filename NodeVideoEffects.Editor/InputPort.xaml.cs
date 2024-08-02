using NodeVideoEffects.Control;
using NodeVideoEffects.Type;
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

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for OutputPort.xaml
    /// </summary>
    public partial class InputPort : UserControl
    {
        private IControl? control;
        private Input? _input;

        public event PropertyChangedEventHandler PropertyChanged;
        public InputPort(Input input)
        {
            InitializeComponent();
            _input = input;
            portName.Content = input.Name;
            control = input.Control;
            control.PropertyChanged += OnControlPropertyChanged;
            portControl.Content = control;            
        }

        public object? Value
        {
            get => control?.Value;
            set
            {
                if (control != null)
                    control.Value = value;
            }
        }

        private void OnControlPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _input.Value = control.Value;
            PropertyChanged?.Invoke(_input, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}
