using Node.Editor.Control;

namespace Node.Editor.Attributes;

public class BoolPortControlAttribute : PropertyControlBaseAttribute
{
    public override Type ControlType => typeof(BoolPort);

    public bool Default { get; set; } = false;

    public override object GetDefaultValue()
    {
        return Default;
    }
}