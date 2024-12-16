using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Output port
    /// </summary>
    public class Output : IDisposable
    {
        private PortValue _value;
        private Object? _result = null;
        private bool _isSuccess = false;
        private List<Connection> _connection = new();
        private String _name;

        /// <summary>
        /// Create new output port object
        /// </summary>
        /// <param name="value">PortValue</param>
        /// <param name="name">This port's name</param>
        public Output(PortValue value, string name)
        {
            _value = value;
            _name = name;
        }

        /// <summary>
        /// Value of this port
        /// </summary>
        public Object? Value { get { return _isSuccess ? _result : null; } set { _value.SetValue(value); _result = _value.Value; IsSuccess = true; } }

        public Color Color { get => _value.Color; }

        public System.Type Type { get => _value.Type; }

        /// <summary>
        /// Name of this output port
        /// </summary>
        public String Name { get { return _name; } }

        /// <summary>
        /// List of node id and output port connected to this port
        /// </summary>
        public List<Connection> Connection { get { return _connection; } }

        /// <summary>
        /// Was the calculation successful
        /// </summary>
        public bool IsSuccess
        {
            get { return _isSuccess; }
            set
            {
                _isSuccess = value;
                if (_connection.Count != 0)
                {
                    foreach (Connection connection in _connection)
                    {
                        NodesManager.NoticeOutputChanged(connection.id, connection.index);
                    }
                }
            }
        }

        /// <summary>
        /// Set node and input port connected to this port
        /// </summary>
        /// <param name="id">ID of node will be connected to this port</param>
        /// <param name="index">Index of input port will be connected to this port</param>
        public void AddConnection(string id, int index)
        {
            _connection.Add(new(id, index));
        }

        /// <summary>
        /// Remove node and input port connected to this port
        /// </summary>
        /// <param name="id">ID of node was connected to this port</param>
        /// <param name="index">Index of input port was connected to this port</param>
        public void RemoveConnection(string id, int index)
        {
            _connection.Remove(new(id, index));
        }

        public void Dispose()
        {
            _value.Dispose();
        }
    }
}
