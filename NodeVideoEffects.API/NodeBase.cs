using System.Windows.Media;
using NodeVideoEffects.Type;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1;

namespace NodeVideoEffects.API;

public abstract class NodeBase : INode
{
    private readonly string _shaderId = "";
    private readonly string _effectId;
    private readonly List<(System.Type, string)>? _properties;
    private VideoEffectsLoader? _videoEffect;
    
    protected NodeBase(Input[] inputs, Output[] outputs, string name, Color color, string category, string id) : base(inputs,
        outputs, name, color, category)
    {
        _effectId = id;
    }
    
    protected NodeBase(Input[] inputs, Output[] outputs, string name, Color color, string category, string id,
        string shaderName, List<(System.Type, string)> properties) :
        base(inputs, outputs, name, color, category)
    {
        _effectId = id;
        if (id == "") return;
        _shaderId = VideoEffectsLoader.RegisterShader(shaderName);
        _properties = properties;
    }
    
    public override async Task Calculate()
    {
        var result = await CalculateImpl();
        if (result.Count != Outputs.Length) Logger.Write(LogLevel.Error, "Result count does not match output count");
        _ = result.Select((obj, i) =>
        {
            var type = Outputs[i].Type;
            return type switch
            {
                not null when type == typeof(ImageWrapper) => Outputs[i].Value = new ImageWrapper(obj as ID2D1Image),
                _ => Outputs[i].Value = obj
            };
        });
    }

    protected abstract Task<List<object?>> CalculateImpl();

    protected async Task<ID2D1Image?> RunEffect(ID2D1Image? input, params object?[] values)
    {
        (_videoEffect ??= await VideoEffectsLoader.LoadEffect(_properties!, _shaderId, _effectId))
            .SetValue(_properties!.Select((property, i) => Convert.ChangeType(values[i], property.Item1)))
            .Update(input, out var output);
        return output;
    }
}