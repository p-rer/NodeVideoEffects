namespace NodeVideoEffects.Type
{
    public record struct NodeInfo
    {
        public string ID { get; set; }
        public string Type { get; }
        public List<object?> Values { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public List<Connection> Connections { get; set; }

        public NodeInfo(string id, System.Type type, List<object?> values, double x, double y, List<Connection> connections)
        {
            ID = id;
            Type = $"{type.FullName}, {type.Assembly}";
            Values = values;
            X = x;
            Y = y;
            Connections = connections;
        }

        public NodeInfo DeepCopy()
        {
            return new NodeInfo(ID,
                                System.Type.GetType(Type)!,
                                Values?.Select(value => value is ICloneable cloneable ? cloneable.Clone() : value).ToList() ?? [],
                                X,
                                Y,
                                Connections ?? new List<Connection>());
        }

        public override int GetHashCode() => base.GetHashCode();
        public override string? ToString() => base.ToString();
    }
}
