using Newtonsoft.Json;
using Vortice.Direct2D1;

namespace Node.ValueTypes;

public class MaskWrapper
{
    [JsonIgnore] public ID2D1Image? Mask { get; set; }

    public override string ToString()
    {
        return Mask?.NativePointer.ToString("x8") ?? "NULL";
    }
}