using System.Windows.Media;
using NodeVideoEffects.Type;
using NodeVideoEffects.Utility;
using Vortice.Direct2D1;

namespace NodeVideoEffects.API;

/// <summary>
/// Base class for all nodes
/// </summary>
public abstract class NodeBase : INode
{
    /// <summary>
    /// Initialize a new instance of <see cref="NodeBase"/>
    /// </summary>
    /// <param name="inputs">Array of <see cref="Input"/>. Input ports of this node</param>
    /// <param name="outputs">Array of <see cref="Output"/>. Output ports of this node</param>
    /// <param name="name">Name of this node</param>
    /// <param name="color">Color of this node. Nodes with the same function should be the same color.</param>
    /// <param name="category">Category of this node. Categories can be nested by separating them with “/”.</param>
    protected NodeBase(Input[] inputs, Output[] outputs, string name, Color color, string category) : base(inputs,
        outputs, name, color, category)
    {
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

    /// <summary>
    /// Core part of node
    /// </summary>
    /// <returns>List of values to be set for outputs.</returns>
    protected abstract Task<List<object?>> CalculateImpl();
}