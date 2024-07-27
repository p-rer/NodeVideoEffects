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

        public static bool IsSuccessOutput(string id, int index) => _dictionary[id].Outputs[index].IsSuccess;

        public static void AddNode(string id, INode node) => _dictionary.Add(id, node);
        public static void RemoveNode(string id) => _dictionary.Remove(id);
    }
}
