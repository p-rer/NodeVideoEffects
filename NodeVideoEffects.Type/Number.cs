using NodeVideoEffects.Control;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    public class Number : PortValue
    {
        private double _value;
        private double _default;
        private double _min;
        private double _max;
        private int _digits;

        /// <summary>
        /// Create new number object
        /// </summary>
        /// <param name="_default">Default number</param>
        /// <param name="_min">Min value</param>
        /// <param name="_max">Max value</param>
        /// <param name="_digits">Number of decimal places</param>
        public Number(double _default, double? _min, double? _max, int? _digits)
        {
            this._min = _min ?? Double.NaN;
            this._max = _max ?? Double.NaN;
            this._default = _default;
            this._value = _default;
            this._digits = _digits ?? 8;
        }

        public System.Type Type { get => typeof(double); }

        public Color Color { get => Colors.Coral; }

        /// <summary>
        /// Double value
        /// </summary>
        public object Value { get { return _value; } }

        public void _SetValue(object value)
        {
            if (Convert.ToDouble(value) == Double.NaN) value = _default;
            else
            {
                if (_min != Double.NaN && Convert.ToDouble(value) < _min) value = _min;
                if (_max != Double.NaN && Convert.ToDouble(value) > _max) value = _max;
            }

            _value = Math.Round(Convert.ToDouble(value), _digits);
        }

        public IControl? Control => new NumberPort(_default, _value, _min, _max, _digits);
    }
}
