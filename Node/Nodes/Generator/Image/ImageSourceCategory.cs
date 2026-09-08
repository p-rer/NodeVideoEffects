using System.Windows.Media;
using Node.Graph;

namespace Node.Nodes.Generator.Image;

public sealed class ImageSourceCategory : INodeCategory
{
    public string Category => "NodeEffectKey_GeneratorCategoryName/NodeEffectKey_ImageSourceCategoryName";
    public string Color => nameof(Colors.MediumSeaGreen);
}