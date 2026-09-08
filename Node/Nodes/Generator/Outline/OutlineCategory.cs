using System.Windows.Media;
using Node.Graph;

namespace Node.Nodes.Generator.Outline;

public sealed class OutlineCategory : INodeCategory
{
    public string Category => "NodeEffectKey_GeneratorCategoryName/NodeEffectKey_OutlineCategoryName";
    public string Color => nameof(Colors.Goldenrod);
}