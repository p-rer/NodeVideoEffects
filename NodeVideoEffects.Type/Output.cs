namespace NodeVideoEffects.Type
{
    public class Output
    {
        private PortValue _value;
        private Object? _result = null;
        private bool _isSuccess = false;
        private List<Connection> _connection = new();
        private String _name;

        public Output(PortValue value, string name)
        {
            _value = value;
            _name = name;
        }

        public Object? Value { get { return _isSuccess ? _result : null; } set { _value.SetValue(value); _result = _value.Value; _isSuccess = true; } }
        public System.Type Type { get { return _value.Type; } }
        public String Name { get { return _name; } }
        public List<Connection> Connection { get { return _connection; } }
        public bool IsSuccess
        {
            get { return _isSuccess; }
            set
            {
                _isSuccess = value;
                if (!_isSuccess && !(_connection.Count == 0))
                {
                    foreach (Connection connection in _connection)
                    {
                        NodesManager.NoticeOutputChanged(connection.id, connection.index);
                    }
                }
            }
        }

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
