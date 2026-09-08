using Node.Editor.Control;

namespace Node.Editor.Attributes;

public class TextPortControlAttribute : PropertyControlBaseAttribute
{
    public override Type ControlType => typeof(TextPort);

    /// <summary>
    ///     デフォルト値
    /// </summary>
    public string Default { get; set; } = "";

    public override object GetDefaultValue()
    {
        return Default;
    }
}