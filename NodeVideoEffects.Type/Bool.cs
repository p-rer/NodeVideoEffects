﻿namespace NodeVideoEffects.Type
{
    public class Bool : PortValue
    {
        bool _value;
        public Bool(bool _value)
        {
            this._value = _value;
        }

        public object Value { get { return _value; } }
        public System.Type Type { get { return typeof(double); } }

        public void _SetValue(object value)
        {
            _value = (bool)value;
        }
    }
}
