using Newtonsoft.Json;
using Vortice.Direct2D1;

namespace Node.ValueTypes;

public class BrushWrapper
{
    [JsonIgnore] public ID2D1Brush? Brush { get; set; }

    public override string ToString()
    {
        return Brush?.NativePointer.ToString("x8") ?? "NULL";
    }
}