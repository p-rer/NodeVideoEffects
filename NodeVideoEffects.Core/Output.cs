using System.Windows.Media;

namespace NodeVideoEffects.Core;

/// <summary>
/// Output port
/// </summary>
public class Output : IDisposable
{
    private readonly IPortValue _value;
    private object? _result;
    private bool _isSuccess;
    private readonly bool _noCache;

    /// <summary>
    /// Create new output port object
    /// </summary>
    /// <param name="value">PortValue</param>
    /// <param name="name">This port's name</param>
    /// <param name="noCache">If true, this port will not cache the value</param>
    public Output(IPortValue value, string name, bool noCache = false)
    {
        _value = value;
        Name = name;
        _noCache = noCache;
    }

    /// <summary>
    /// Value of this port
    /// </summary>
    public object? Value
    {
        get => !_noCache & _isSuccess ? _result : null;
        set
        {
            IsSuccess = !_noCache & true;
            _value.SetValue(value);
            _result = _value.Value;
        }
    }

    public Color Color => _value.Color;

    public Type Type => _value.Type;

    /// <summary>
    /// Name of this output port
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// List of node id and output port connected to this port
    /// </summary>
    public List<PortInfo> Connection { get; } = new();

    /// <summary>
    /// Was the calculation successful
    /// </summary>
    public bool IsSuccess
    {
        get => !_noCache & _isSuccess;
        set
        {
            _isSuccess = !_noCache & value;
            if (Connection.Count == 0) return;
            foreach (var connection in Connection) NodesManager.NotifyOutputChanged(connection.Id, connection.Index);
        }
    }

    internal void SetIsSuccess(bool isSuccess, bool isChangedByControl)
    {
        _isSuccess = !_noCache & isSuccess;
        if (Connection.Count == 0) return;
        foreach (var connection in Connection) NodesManager.NotifyOutputChanged(connection.Id, connection.Index, isChangedByControl);
    }

    /// <summary>
    /// Set node and input port connected to this port
    /// </summary>
    /// <param name="id">ID of node will be connected to this port</param>
    /// <param name="index">Index of input port will be connected to this port</param>
    public void AddConnection(string id, int index)
    {
        Connection.Add(new PortInfo(id, index));
    }

    /// <summary>
    /// Remove node and input port connected to this port
    /// </summary>
    /// <param name="id">ID of node was connected to this port</param>
    /// <param name="index">Index of input port was connected to this port</param>
    public void RemoveConnection(string id, int index)
    {
        Connection.Remove(new PortInfo(id, index));
    }

    public void Dispose()
    {
        _value.Dispose();
    }
}