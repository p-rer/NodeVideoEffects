using System.Windows.Media;
using Node.Graph;

namespace Node.Nodes.Func;

public sealed class FunctionCategory : INodeCategory
{
    public string Category => "NodeEffectKey_FunctionsCategoryName";
    public string Color => nameof(Colors.SlateGray);
}