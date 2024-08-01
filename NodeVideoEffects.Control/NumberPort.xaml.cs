using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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

        private bool isClicking = false;
        private bool isDragging = false;
        private bool isEditing = false;
        private Point startPoint;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

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

        private void box_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isEditing)
            {
                startPoint = box.PointToScreen(e.GetPosition(box));
                isClicking = true;
                box.Focusable = false;
                Mouse.OverrideCursor = Cursors.None;
                e.Handled = true;
                box.CaptureMouse();
            }
        }

        private void box_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isClicking && !isEditing)
            {
                Point currentPoint = box.PointToScreen(e.GetPosition(box));
                double delta = currentPoint.X - startPoint.X;

                if (Math.Abs(delta) > SystemParameters.MinimumHorizontalDragDistance || isDragging)
                {
                    isDragging = true;

                    double sensitivity = 0.01;
                    SetCursorPos((int)startPoint.X, (int)startPoint.Y);
                    _value = Math.Round(_value += delta * sensitivity, _dig);
                    box.Text = _value.ToString("F" + _dig);
                }
                e.Handled = true;
            }
        }

        private void box_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isClicking = false;

            if (isDragging)
            {
                isDragging = false;
                e.Handled = true;
            }
            else
            {
                isEditing = true;
                box.Focusable = true;
                box.Focus();
            }
            Mouse.OverrideCursor = null;
            box.ReleaseMouseCapture();
        }

        private void box_LostFocus(object sender, RoutedEventArgs e)
        {
            isEditing = false;
            SetCursorPos((int)startPoint.X, (int)startPoint.Y);
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
                if (box.Text == "")
                {
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
                _value = Math.Round(value, _dig);
                box.Text = _value.ToString("F" + _dig);

                Keyboard.ClearFocus();
                isEditing = false;
            }
        }
    }
}
