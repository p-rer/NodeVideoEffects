using System.Windows.Media;

namespace Node.Editor.Attributes;

public class PortColorSettingAttribute(string color = nameof(Colors.SlateGray)) : Attribute
{
    public string Color { get; private set; } = color;
}