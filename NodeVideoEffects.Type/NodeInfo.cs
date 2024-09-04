using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeVideoEffects.Type
{
    public record NodeInfo
    {
        public string ID { get; set; }
        public string Type { get; }
        public List<object> Values { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public List<Connection> Connections { get; set; }

        public NodeInfo(string id, System.Type type, List<object> values, double x, double y, List<Connection> connections)
        {
            ID = id;
            Type = $"{type.FullName}, {type.Assembly}";
            Values = values;
            X = x;
            Y = y;
            Connections = connections;
        }

        public override int GetHashCode() => base.GetHashCode();
        public override string? ToString() => base.ToString();
        public virtual bool Equals(NodeInfo? other) =>
            other != null &&
            ID == other.ID &&
            Type == other.Type &&
            Values.SequenceEqual(other.Values) &&
            X == other.X &&
            Y == other.Y &&
            Connections.SequenceEqual(other.Connections);
    }
}
