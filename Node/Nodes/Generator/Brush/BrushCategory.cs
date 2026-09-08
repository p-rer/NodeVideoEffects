using System.Windows.Media;
using Node.Graph;

namespace Node.Nodes.Generator.Brush;

public sealed class BrushCategory : INodeCategory
{
    public string Category =>
        "NodeEffectKey_GeneratorCategoryName/NodeEffectKey_BrushCategoryName";

    public string Color => nameof(Colors.LawnGreen);
}