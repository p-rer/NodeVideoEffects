namespace NodeVideoEffects.Core;

/// <summary>
/// Pair of node ID and port index
/// </summary>
public record struct PortInfo
{
    /// <summary>
    /// Node ID
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Port index
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="id">Node ID</param>
    /// <param name="index">Port index</param>
    public PortInfo(string id, int index) : this()
    {
        (Id, Index) = (id, index);
    }

    public PortInfo()
    {
        Id = "";
        Index = 0;
    }
}