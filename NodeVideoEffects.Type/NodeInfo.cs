namespace NodeVideoEffects.Type
{
    public record struct NodeInfo
    {
        public string Id { get; set; }
        public string Type { get; }
        public List<object?> Values { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public List<Connection> Connections { get; init; }

        public NodeInfo(string id, System.Type type, List<object?> values, double x, double y, List<Connection> connections)
        {
            Id = id;
            Type = $"{type.FullName}, {type.Assembly}";
            Values = values;
            X = x;
            Y = y;
            Connections = connections;
        }

        public NodeInfo DeepCopy()
        {
            return new NodeInfo(Id,
                                System.Type.GetType(Type)!,
                                Values?.Select(value => value is ICloneable cloneable ? cloneable.Clone() : value).ToList() ?? [],
                                X,
                                Y,
                                Connections ?? []);
        }

        public override string? ToString() => base.ToString();
    }
}
