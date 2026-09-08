using System.Windows.Media;
using Node.Graph;

namespace Node.Nodes.Effect.Transform;

public class TransformCategory : INodeCategory
{
    public string Category => "NodeEffectKey_EffectCategoryName/NodeEffectKey_TransformCategoryName";
    public string Color => nameof(Colors.DarkCyan);
}