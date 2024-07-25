namespace NodeVideoEffects.Type
{
    public class Number : PortValue
    {
        private double _value;
        private double _default;
        private double _min;
        private double _max;
        private int _digits;

        public Number(double _default, double? _min, double? _max, int? _digits)
        {
            this._min = _min ?? Double.NaN;
            this._max = _max ?? Double.NaN;
            this._default = _default;
            this._value = _default;
            this._digits = _digits ?? 8;
        }

        public object Value { get { return _value; } }
        public System.Type Type { get { return typeof(double); } }

        public void _SetValue(object value)
        {
            if ((double)value == Double.NaN) value = _default;
            else
            {
                if (_min != Double.NaN && (double)value < _min) value = _min;
                if (_max != Double.NaN && (double)value > _max) value = _max;
            }

            _value = Math.Round((double)value, _digits);
        }
    }
}
