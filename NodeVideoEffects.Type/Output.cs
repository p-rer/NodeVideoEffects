using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeVideoEffects.Type
{
    public class Output
    {
        private PortValue _value;
        private Connection[] _connection;

        public Output(PortValue value)
        {
            _value = value;
        }

        public Object Value { get { return _value.Value; } set { _value.SetValue(value); } }
        public System.Type Type { get { return _value.Type; } }
        public Connection[] Connection { get { return _connection; } set { _connection = value; } }
    }
}
