using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Output port
    /// </summary>
    public class Output : IDisposable
    {
        private readonly PortValue _value;
        private object? _result;
        private bool _isSuccess;

        /// <summary>
        /// Create new output port object
        /// </summary>
        /// <param name="value">PortValue</param>
        /// <param name="name">This port's name</param>
        public Output(PortValue value, string name)
        {
            _value = value;
            Name = name;
        }

        /// <summary>
        /// Value of this port
        /// </summary>
        public object? Value { get => _isSuccess ? _result : null;
            set { _value.SetValue(value); _result = _value.Value; IsSuccess = true; } }

        public Color Color => _value.Color;

        public System.Type Type => _value.Type;

        /// <summary>
        /// Name of this output port
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// List of node id and output port connected to this port
        /// </summary>
        public List<Connection> Connection { get; } = new();

        /// <summary>
        /// Was the calculation successful
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                _isSuccess = value;
                if (Connection.Count == 0) return;
                foreach (var connection in Connection)
                {
                    NodesManager.NoticeOutputChanged(connection.Id, connection.Index);
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
            Connection.Add(new Connection(id, index));
        }

        /// <summary>
        /// Remove node and input port connected to this port
        /// </summary>
        /// <param name="id">ID of node was connected to this port</param>
        /// <param name="index">Index of input port was connected to this port</param>
        public void RemoveConnection(string id, int index)
        {
            Connection.Remove(new Connection(id, index));
        }

        public void Dispose()
        {
            _value.Dispose();
        }
    }
}
