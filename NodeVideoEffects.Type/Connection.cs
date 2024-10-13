namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Pair of node ID and port index
    /// </summary>
    public struct Connection
    {
        /// <summary>
        /// Node ID
        /// </summary>
        public string id = "";
        /// <summary>
        /// Port index
        /// </summary>
        public int index = 0;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <param name="index">Port index</param>
        public Connection(string id, int index) : this() => (this.id, this.index) = (id, index);

        public Connection() { id = ""; index = 0; }
    }
}
