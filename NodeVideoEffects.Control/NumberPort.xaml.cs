using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.Control
{
    /// <summary>
    /// Interaction logic for NumberPort.xaml
    /// </summary>
    public sealed partial class NumberPort : IControl
    {
        private readonly float _def;
        private float _value;
        private readonly float _min;
        private readonly float _max;
        private readonly int _dig;

        private bool _isClicking;
        private bool _isDragging;
        private bool _isEditing;
        private Point _startPoint;

        public event PropertyChangedEventHandler? PropertyChanged;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public object? Value { get => _value; set => Update((float?)value ?? _def); }

        public NumberPort(float def, float value, float min, float max, int dig)
        {
            InitializeComponent();

            _def = def;
            _value = value;
            _min = min;
            _max = max;
            _dig = dig;
            Box.Text = Math.Round(_value, _dig).ToString("F" + _dig);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Update()
        {
            float v;
            if (Box.Text == "")
            {
                v = _def;
            }
            else
            {
                try
                {
                    v = float.Parse(Box.Text);
                }
                catch (Exception)
                {
                    v = _value;
                }
            }
            Update(v);
        }

        private void Update(float value)
        {
            var v = value;
            if (!float.IsNaN(_min) && value < _min) v = _min;
            if (!float.IsNaN(_max) && value > _max) v = _max;
            _value = (float)Math.Round(v, _dig);
            Box.Text = _value.ToString("F" + _dig);
            OnPropertyChanged(nameof(Value));
            Keyboard.ClearFocus();
            Box.Focusable = false;
            if(!_isDragging)
                Mouse.OverrideCursor = null;
        }

        private void Box_PreviewMouseDown(object _, MouseButtonEventArgs e) => e.Handled = true;

        private void Box_PreviewMouseLeftButtonDown(object _, MouseButtonEventArgs e)
        {
            _isClicking = true;
            if (_isEditing) return;
            _startPoint = Box.PointToScreen(e.GetPosition(Box));
            e.Handled = true;
            Box.Focusable = false;
            Mouse.OverrideCursor = Cursors.None;
            Box.CaptureMouse();
        }

        private void Box_PreviewMouseMove(object _, MouseEventArgs e)
        {
            try
            {
                if (!_isClicking || _isEditing) return;
                var currentPoint = Box.PointToScreen(e.GetPosition(Box));
                var delta = currentPoint.X - _startPoint.X;

                if (Math.Abs(delta) > SystemParameters.MinimumHorizontalDragDistance || _isDragging)
                {
                    _isDragging = true;

                    const float sensitivity = 0.01f;
                    Update(_value + (float)delta * sensitivity);
                    SetCursorPos((int)_startPoint.X, (int)_startPoint.Y);
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                Box.ReleaseMouseCapture();
                Logger.Write(LogLevel.Error, ex.Message, ex);
            }
        }

        private void Box_PreviewMouseLeftButtonUp(object _, MouseButtonEventArgs e)
        {
            _isClicking = false;
            Mouse.OverrideCursor = null;
            Box.ReleaseMouseCapture();

            if (_isDragging)
            {
                _isDragging = false;
                SetCursorPos((int)_startPoint.X, (int)_startPoint.Y);
                e.Handled = true;
            }
            else
            {
                _isEditing = true;
                Box.Focusable = true;
                Keyboard.Focus(Box);
            }
            OnPropertyChanged(nameof(Value));
        }

        private void Box_LostFocus(object sender, RoutedEventArgs e)
        {
            _isEditing = false;
            Update();
        }

        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = NumberRegex().IsMatch(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string?)e.DataObject.GetData(typeof(string)) ?? string.Empty;
                if (NumberRegex().IsMatch(text))
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
            if (e.Key is not Key.Return) return;
            Update();
            _isEditing = false;
        }

        [GeneratedRegex("[^0-9.-]+")]
        private static partial Regex NumberRegex();
    }
}
