﻿using NodeVideoEffects.Control;
using System.ComponentModel;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Input port
    /// </summary>
    public sealed class Input : INotifyPropertyChanged, IDisposable
    {
        private readonly IPortValue _value;
        private PortInfo _portInfo = new();
        
        private readonly Lock _locker = new();

        /// <summary>
        /// Create new input port object
        /// </summary>
        /// <param name="value">PortValue</param>
        /// <param name="name">This port's name</param>
        public Input(IPortValue value, string name)
        {
            _value = value;
            Name = name;
        }

        /// <summary>
        /// Get or set value to input
        /// </summary>
        public object? Value
        {
            get
            {
                if (_portInfo.Id == "") return _value.Value;
                try
                {
                    lock(_locker)
                    {
                        var task = NodesManager.GetOutputValue(_portInfo.Id, _portInfo.Index);
                        return task.GetAwaiter().GetResult();
                    }
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (_value == value) return;
                _value.SetValue(value);
                OnPropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// Type of input value
        /// </summary>
        public System.Type Type => _value.Type;

        public Color Color => _value.Color;

        /// <summary>
        /// Name of this input port
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Control of this input port
        /// </summary>
        public IControl Control => _value.Control;

        /// <summary>
        /// Node id and output port connected to this port
        /// </summary>
        public PortInfo PortInfo => _portInfo;

        public void UpdatedConnectionValue()
        {
            OnPropertyChanged(nameof(Value));
        }

        /// <summary>
        /// Set node and output port connected to this port
        /// </summary>
        /// <param name="inId">ID of node contains this port</param>
        /// <param name="inIndex">Index of this port</param>
        /// <param name="outId">ID of node will be connected to this port</param>
        /// <param name="outIndex">Index of output port will be connected to this port</param>
        public void SetConnection(string inId, int inIndex, string outId, int outIndex)
        {
            _portInfo = new PortInfo(outId, outIndex);
            if (outId != "")
                NodesManager.NotifyInputConnectionAdd(inId, inIndex, outId, outIndex);
        }

        /// <summary>
        /// Remove connection
        /// </summary>
        /// <param name="id">ID of node contains this port</param>
        /// <param name="index">Index of this port</param>
        public void RemoveConnection(string id, int index)
        {
            if (_portInfo.Id == "") return;
            NodesManager.NotifyInputConnectionRemove(id, index, _portInfo.Id, _portInfo.Index);
            _portInfo.Id = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose() => _value.Dispose();
    }
}
