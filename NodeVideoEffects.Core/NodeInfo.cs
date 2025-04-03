using Newtonsoft.Json;

namespace NodeVideoEffects.Core;

public record struct NodeInfo(
    string Id,
    Type Type,
    List<object?> Values,
    double X,
    double Y,
    List<PortInfo> Connections)
{
    public Type Type { get; init; } = Type;

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

    public override string? ToString()
    {
        return base.ToString();
    }

    public class TypeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Type);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            return reader.Value is string typeName ? Type.GetType(typeName) : null;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is Type type) writer.WriteValue(type.AssemblyQualifiedName);
            else writer.WriteNull();
        }
    }
}