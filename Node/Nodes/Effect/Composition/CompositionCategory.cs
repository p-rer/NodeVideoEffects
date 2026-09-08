using System.Windows.Media;
using Node.Graph;

namespace Node.Nodes.Effect.Composition;

public class CompositionCategory : INodeCategory
{
    public string Category => "NodeEffectKey_EffectCategoryName/NodeEffectKey_CompositionCategoryName";
    public string Color => nameof(Colors.DarkViolet);
}