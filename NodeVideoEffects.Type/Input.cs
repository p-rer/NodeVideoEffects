using System.ComponentModel;
using System.Security.Cryptography;

namespace NodeVideoEffects.Type
{
    public class Input : INotifyPropertyChanged
    {
        private PortValue _value;
        private Connection? _connection;
        private String _name;

        public Input(PortValue value, string name)
        {
            _value = value;
            _name = name;
        }

        public Object? Value
        {
            get
            {
                if (_connection != null)
                {
                    Task<object> task = NodesManager.GetOutputValue(_connection.Value.id, _connection.Value.index);
                    task.Wait();
                    return task.Result;
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
        public System.Type Type { get { return _value.Type; } }
        public String Name { get { return _name; } }
        public Connection? Connection { get { return _connection; } }

        public void UpdatedConnectionValue()
        {
            OnPropertyChanged(nameof(Value));
        }

        public void SetConnection(string iid, int iindex, string oid, int oindex)
        {
            _connection = new(oid, oindex);
            NodesManager.NoticeInputConnectionAdd(iid, iindex, oid, oindex);
        }

        public void RemoveConnection(string id, int index)
        {
            NodesManager.NoticeInputConnectionRemove(id, index, _connection.Value.id, _connection.Value.index);
            _connection = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
