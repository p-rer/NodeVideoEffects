using System.ComponentModel;
using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Core;

/// <summary>
/// Input port
/// </summary>
public sealed class Input : INotifyPropertyChanged, IDisposable
{
    private readonly Lock _locker = new();
    private bool _disposed;
    private PortInfo _portInfo = new();

    /// <summary>
    /// Create new input port object
    /// </summary>
    /// <param name="value">PortValue</param>
    /// <param name="name">This port's name</param>
    public Input(IPortValue value, string name)
    {
        _disposed = false;
        PortValue = value;
        Name = name;
    }

    /// <summary>
    /// Get or set value to input
    /// </summary>
    public object? Value
    {
        get
        {
            if (_disposed || _portInfo.Id == "") return PortValue.Value;
            lock (_locker)
            {
                var task = NodesManager.GetOutputValue(_portInfo.Id, _portInfo.Index);
                var result = task.GetAwaiter().GetResult();
                return result;
            }
        }
        set
        {
            if (PortValue == value) return;
            PortValue.SetValue(value);
            OnPropertyChanged(nameof(Value), _portInfo.Id == "");
        }
    }

    public IPortValue PortValue { get; }

    public object? DefaultValue => PortValue.Value;

    /// <summary>
    /// Type of input value
    /// </summary>
    public Type Type => PortValue.Type;

    public Color Color => PortValue.Color;

    /// <summary>
    /// Name of this input port
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Control of this input port
    /// </summary>
    public IControl Control => PortValue.Control;

    /// <summary>
    /// Node id and output port connected to this port
    /// </summary>
    public PortInfo PortInfo => _portInfo;

    public void Dispose()
    {
        PortValue.Dispose();
        _disposed = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdatedConnectionValue(bool isControlChanged)
    {
        if (!isControlChanged)
            return;
        OnPropertyChanged(nameof(Value), false);
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
        if (id == "")
            _portInfo.Id = "";
        if (_portInfo.Id == "") return;
        NodesManager.NotifyInputConnectionRemove(id, index, _portInfo.Id, _portInfo.Index);
        _portInfo.Id = "";
    }

    private void OnPropertyChanged(string propertyName, bool isChangedByControl)
    {
        PropertyChanged?.Invoke(isChangedByControl, new PropertyChangedEventArgs(propertyName));
    }
}