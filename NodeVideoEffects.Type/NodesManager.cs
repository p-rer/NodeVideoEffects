namespace NodeVideoEffects.Type
{
    public static class NodesManager
    {
        private static Dictionary<string, INode>? _dictionary = new();

        public static async Task<Object> GetOutputValue(string id, int index)
        {
            INode node = _dictionary[id];
            if (node.Outputs[index].IsSuccess == true)
                return node.Outputs[index].Value;
            await node.Calculate();
            return node.Outputs[index].Value;
        }

        public static void NoticeOutputChanged(string id, int index)
        {
            _dictionary[id].Inputs[index].UpdatedConnectionValue();
        }
        public static void NoticeInputConnectionAdd(string iid, int iindex, string oid, int oindex)
        {
            _dictionary[oid].Outputs[oindex].Connection.Add(new(iid, iindex));
        }
        public static void NoticeInputConnectionRemove(string iid, int iindex, string oid, int oindex)
        {
            _dictionary[oid].Outputs[oindex].Connection.Remove(new(iid, iindex));
        }
        public static void AddNode(string id, INode node) => _dictionary.Add(id, node);
        public static void RemoveNode(string id) => _dictionary.Remove(id);
    }
}
