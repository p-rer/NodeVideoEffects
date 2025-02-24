using Newtonsoft.Json;

namespace NodeVideoEffects.Type
{
    public record struct NodeInfo(
        string Id,
        System.Type Type,
        List<object?> Values,
        double X,
        double Y,
        List<PortInfo> Connections)
    {
        public System.Type Type { get; init; } = Type;

        public List<object?> Values { get; init; } = Values;
        public double X { get; init; } = X;
        public double Y { get; init; } = Y;
        public List<PortInfo> Connections { get; init; } = Connections;

        public NodeInfo DeepCopy()
        {
            return this with
            {
                Values =
                Values?.Select(value => value is ICloneable cloneable ? cloneable.Clone() : value).ToList() ?? [],
                Connections = Connections ?? []
            };
        }

        public override string? ToString() => base.ToString();

        public class TypeJsonConverter : JsonConverter
        {
            public override bool CanConvert(System.Type objectType)
            {
                return objectType == typeof(System.Type);
            }

            public override object? ReadJson(JsonReader reader, System.Type objectType, object? existingValue,
                JsonSerializer serializer) => reader.Value is string typeName ? System.Type.GetType(typeName) : null;

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is System.Type type) writer.WriteValue(type.AssemblyQualifiedName); else writer.WriteNull();
            }
        }
    }
}
