using System.Windows.Media;
using NodeVideoEffects.Type;
using Vortice.Direct2D1;

namespace NodeVideoEffects.API;

/// <summary>
/// Base class for nodes that use shaders
/// </summary>
public abstract class ShaderEffectNodeBase : NodeLogic
{
    private readonly string _shaderId = "";
    private readonly string _effectId = "";
    private readonly List<(System.Type, string)>? _properties;
    private VideoEffectsLoader? _videoEffect;
    
    /// <summary>
    /// Initialize a new instance of <see cref="NodeLogic"/>
    /// </summary>
    /// <param name="inputs">Array of <see cref="Input"/>. Input ports of this node</param>
    /// <param name="outputs">Array of <see cref="Output"/>. Output ports of this node</param>
    /// <param name="name">Name of this node</param>
    /// <param name="color">Color of this node. Nodes with the same function should be the same color.</param>
    /// <param name="category">Category of this node. Categories can be nested by separating them with “/”.</param>
    /// <param name="id">ID of the effect given when a class inheriting from this class has only a string argument.</param>
    /// <param name="shaderName">Name of compiled shader file registered in resource</param>
    /// <param name="properties">Collection of effect property types and name</param>
    protected ShaderEffectNodeBase(Input[] inputs, Output[] outputs, string name, Color color, string category, string id,
        string shaderName, List<(System.Type, string)> properties)
        : base(inputs, outputs, name, color, category)
    {
        if (id == "") return;
        _effectId = id;
        _shaderId = VideoEffectsLoader.RegisterShader(shaderName);
        _properties = properties;
    }

    /// <summary>
    /// Execute shaders
    /// </summary>
    /// <param name="input">Input image given to shader</param>
    /// <param name="values">Arguments given to shaders</param>
    /// <returns>Output image</returns>
    protected async Task<ID2D1Image?> RunEffect(ID2D1Image? input, params object?[] values)
    {
        (_videoEffect ??= await VideoEffectsLoader.LoadEffect(_properties!, _shaderId, _effectId))
            .SetValue(_properties!.Select((property, i) => Convert.ChangeType(values[i], property.Item1)))
            .Update(input, out var output);
        return output;
    }
}