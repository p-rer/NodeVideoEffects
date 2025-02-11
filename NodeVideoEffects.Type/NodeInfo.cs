namespace NodeVideoEffects.Type
{
    public record struct NodeInfo
    {
        public string Id { get; set; }
        public System.Type Type { get; }
        public List<object?> Values { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public List<PortInfo> Connections { get; init; }

        public NodeInfo(string id, System.Type type, List<object?> values, double x, double y, List<PortInfo> connections)
        {
            Id = id;
            Type = type;
            Values = values;
            X = x;
            Y = y;
            Connections = connections;
        }

        public NodeInfo DeepCopy()
        {
            return new NodeInfo(Id,
                                Type,
                                Values?.Select(value => value is ICloneable cloneable ? cloneable.Clone() : value).ToList() ?? [],
                                X,
                                Y,
                                Connections ?? []);
        }

        public override string? ToString() => base.ToString();
    }
}
