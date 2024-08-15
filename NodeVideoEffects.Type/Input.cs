using NodeVideoEffects.Control;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Input port
    /// </summary>
    public class Input : INotifyPropertyChanged
    {
        private PortValue _value;
        private Connection? _connection;
        private String _name;

        /// <summary>
        /// Create new input port object
        /// </summary>
        /// <param name="value">PortValue</param>
        /// <param name="name">This port's name</param>
        public Input(PortValue value, string name)
        {
            _value = value;
            _name = name;
        }

        /// <summary>
        /// Get or set value to input
        /// </summary>
        /// <exception cref="TypeMismatchException">Type of set value is wrong.</exception>
        public Object? Value
        {
            get
            {
                if (_connection != null)
                {
                    try
                    {
                        Task<object> task = NodesManager.GetOutputValue(_connection.Value.id, _connection.Value.index);
                        task.Wait();
                        return task.Result;
                    }
                    catch
                    {
                        return null;
                    }
                }
                return _value.Value;
            }
            set
            {
                if (_value != value)
                {
                    _value.SetValue(value);
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// Type of input value
        /// </summary>
        public System.Type Type { get { return _value.Type; } }

        public Color Color { get => _value.Color; }

        /// <summary>
        /// Name of this input port
        /// </summary>
        public String Name { get { return _name; } }

        /// <summary>
        /// Control of this input port
        /// </summary>
        public IControl? Control { get => _value.Control; }

        /// <summary>
        /// Node id and output port connected to this port
        /// </summary>
        public Connection? Connection { get { return _connection; } }

        public void UpdatedConnectionValue()
        {
            OnPropertyChanged(nameof(Value));
        }

        /// <summary>
        /// Set node and output port connected to this port
        /// </summary>
        /// <param name="iid">ID of node contains this port</param>
        /// <param name="iindex">Index of this port</param>
        /// <param name="oid">ID of node will be connected to this port</param>
        /// <param name="oindex">Index of output port will be connected to this port</param>
        public void SetConnection(string iid, int iindex, string oid, int oindex)
        {
            _connection = new(oid, oindex);
            if (oid != null)
                NodesManager.NoticeInputConnectionAdd(iid, iindex, oid, oindex);
        }

        /// <summary>
        /// Remove connection
        /// </summary>
        /// <param name="id">ID of node contains this port</param>
        /// <param name="index">Index of this port</param>
        public void RemoveConnection(string id, int index)
        {
            if (_connection != null)
            {
                NodesManager.NoticeInputConnectionRemove(id, index, _connection.Value.id, _connection.Value.index);
                _connection = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
