using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for NumberPort.xaml
    /// </summary>
    public partial class NumberPort : UserControl, IControl
    {
        double _def;
        double _value;
        double _min;
        double _max;
        int _dig;

        bool isDragging = false;
        Point lastPoint;

        public NumberPort(double def, double value, double min, double max, int dig)
        {
            InitializeComponent();

            _def = def;
            _value = value;
            _min = min;
            _max = max;
            _dig = dig;
            box.Text = Math.Round(_value, _dig).ToString("F" + _dig);
        }
        
        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (new Regex("[^0-9.-]+").IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void ValueSubmit(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (box.Text == "") {
                    box.Text = _def.ToString("F" + _dig);
                    _value = _def;
                }
                double value;
                try
                {
                    value = double.Parse(box.Text);
                    if (_min != double.NaN && value < _min) value = _min;
                    if (_max != double.NaN && value > _max) value = _max;
                }
                catch (Exception)
                {
                    value = _value;
                }
                _value = value;
                box.Text = Math.Round(_value, _dig).ToString("F" + _dig);

                Keyboard.ClearFocus();
            }
        }
    }
}
