namespace NodeVideoEffects.Type
{
    public struct Connection
    {
        public string id;
        public int index;

        public Connection(string id, int index) : this() => (this.id, this.index) = (id, index);
    }
}
