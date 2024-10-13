using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public event PropertyChangedEventHandler PropertyChanged;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public object? Value { get => _value; set => Update((double?)value ?? _def); }

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Update()
        {
            double v;
            if (box.Text == "")
            {
                v = _def;
            }
            else
            {
                try
                {
                    v = double.Parse(box.Text);
                }
                catch (Exception)
                {
                    v = _value;
                }
            }
            Update(v);
        }

        private void Update(double value)
        {
            double v;
            v = value;
            if (_min != double.NaN && value < _min) v = _min;
            if (_max != double.NaN && value > _max) v = _max;
            _value = Math.Round(v, _dig);
            box.Text = _value.ToString("F" + _dig);
            OnPropertyChanged(nameof(Value));
            Keyboard.ClearFocus();
            box.Focusable = false;
        }

        private void box_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = box.PointToScreen(e.GetPosition(box));
            if (!isEditing)
            {
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
                    Update(_value + delta * sensitivity);
                    SetCursorPos((int)startPoint.X, (int)startPoint.Y);
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
            SetCursorPos((int)startPoint.X, (int)startPoint.Y);
            Mouse.OverrideCursor = null;
            box.ReleaseMouseCapture();
        }

        private void box_LostFocus(object sender, RoutedEventArgs e)
        {
            isEditing = false;
            Update();
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
                Update();
                isEditing = false;
            }
        }
    }
}
