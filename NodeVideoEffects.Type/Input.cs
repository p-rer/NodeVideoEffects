using System.ComponentModel;

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

        public Object Value
        {
            get { return _value.Value; }
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

        public void AddConnection(string id, int index)
        {
            _connection = new(id, index);
        }

        public void RemoveConnection(string id, int index)
        {
            _connection = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
