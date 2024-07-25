namespace NodeVideoEffects.Type
{
    public class Output
    {
        private PortValue _value;
        private Object? _result = null;
        private List<Connection> _connection;
        private String _name;

        public Output(PortValue value, string name)
        {
            _value = value;
            _name = name;
        }

        public Object? Value { get { return _result; } set { _value.SetValue(value); } }
        public System.Type Type { get { return _value.Type; } }
        public String Name { get { return _name; } }
        public List<Connection> Connection { get { return _connection; } }

        public void AddConnection(string id, int index)
        {
            _connection.Add(new(id, index));
        }

        public void RemoveConnection(string id, int index)
        {
            _connection.Remove(new(id, index));
        }
    }
}
