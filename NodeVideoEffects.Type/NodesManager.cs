namespace NodeVideoEffects.Type
{
    public static class NodesManager
    {
        private static Dictionary<string, INode>? _dictionary = new();

        public static async Task<Object> GetOutputValue(string id, int index)
        {
            INode node = _dictionary[id];
            if (node.Outputs[index].Value != null)
                return node.Outputs[index].Value;
            foreach (Input input in node.Inputs)
            {
                if (input.Value == null)
                    input.Value = GetOutputValue(input.Connection.Value.id, input.Connection.Value.index);
            }
            await node.Calculate();
            return node.Outputs[index].Value;
        }

        public static void AddNode(string id, INode node) => _dictionary.Add(id, node);
        public static void RemoveNode(string id) => _dictionary.Remove(id);
    }
}
