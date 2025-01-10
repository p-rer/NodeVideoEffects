namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Pair of node ID and port index
    /// </summary>
    public record struct Connection
    {
        /// <summary>
        /// Node ID
        /// </summary>
        public string Id = "";
        /// <summary>
        /// Port index
        /// </summary>
        public readonly int Index = 0;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <param name="index">Port index</param>
        public Connection(string id, int index) : this() => (Id, Index) = (id, index);

        public Connection() { Id = ""; Index = 0; }
    }
}
