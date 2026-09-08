namespace Node.Utility;

internal class ShaderResourceUri
{
    public static Uri Get(string shaderName)
    {
        return new Uri($"pack://application:,,,/Node;component/Resources/Shader/{shaderName}.cso");
    }
}