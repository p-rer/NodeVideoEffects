using System.Windows.Media;
using Node.Editor.Control;
using Node.Editor.Converters;

namespace Node.Editor.Attributes;

public class ColorPortControlAttribute : PropertyControlBaseAttribute
{
    public override Type ControlType => typeof(ColorPort);

    public string DefaultColor { get; set; } = ColorStringConverter.ToString(Colors.White);

    public override object GetDefaultValue()
    {
        return ColorStringConverter.ToColor(DefaultColor);
    }
}