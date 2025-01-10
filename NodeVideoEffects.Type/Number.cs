using NodeVideoEffects.Control;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    public class Number : PortValue
    {
        private double _value;
        private readonly double _default;
        private readonly double _min;
        private readonly double _max;
        private readonly int _digits;

        /// <summary>
        /// Create new number object
        /// </summary>
        /// <param name="default">Default number</param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="digits">Number of decimal places</param>
        public Number(double @default, double? min, double? max, int? digits)
        {
            _min = min ?? Double.NaN;
            _max = max ?? Double.NaN;
            _default = @default;
            _value = @default;
            _digits = digits ?? 8;
        }

        public System.Type Type => typeof(double);

        public Color Color => Colors.Coral;

        /// <summary>
        /// Double value
        /// </summary>
        public object Value => _value;

        public void _SetValue(object? value)
        {
            if (double.IsNaN(Convert.ToDouble(value))) value = _default;
            else
            {
                if (!double.IsNaN(_min) && Convert.ToDouble(value) < _min) value = _min;
                if (!double.IsNaN(_max) && Convert.ToDouble(value) > _max) value = _max;
            }

            _value = Math.Round(Convert.ToDouble(value), _digits);
        }

        public void Dispose() { }

        public IControl Control => new NumberPort(_default, _value, _min, _max, _digits);
    }
}
