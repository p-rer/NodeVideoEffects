﻿namespace NodeVideoEffects.Type
{
    public class Output
    {
        private PortValue _value;
        private List<Connection> _connection;
        private String _name;

        public Output(PortValue value, string name)
        {
            _value = value;
            _name = name;
        }

        public Object Value { get { return _value.Value; } set { _value.SetValue(value); } }
        public System.Type Type { get { return _value.Type; } }
        public String Name { get { return _name; } }
        public List<Connection> Connection { get { return _connection; } set { _connection = value; } }
    }
}
