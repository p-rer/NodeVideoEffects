namespace NodeVideoEffects.Type
{
    public class Bool : PortValue
    {
        bool _value;
        /// <summary>
        /// Create new bool object
        /// </summary>
        /// <param name="_value">Value</param>
        public Bool(bool _value)
        {
            this._value = _value;
        }

        /// <summary>
        /// Value
        /// </summary>
        public object Value { get { return _value; } }

        public void _SetValue(object value)
        {
            _value = (bool)value;
        }
    }
}
