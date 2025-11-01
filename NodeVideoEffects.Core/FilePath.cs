using System.Windows.Media;
using NodeVideoEffects.Control;

namespace NodeVideoEffects.Core;

public class FilePath : IPortValue
{
    private readonly List<(string Name, string[] Ext)> _allowExtension;
    private string _fileName;

    /// <summary>
    ///     Create new bool object
    /// </summary>
    /// <param name="defaultPath">default value of ile path</param>
    /// <param name="allowExtension">extension filter</param>
    public FilePath(string defaultPath, List<(string Name, string[] Ext)> allowExtension)
    {
        _fileName = defaultPath;
        _allowExtension = allowExtension;
    }

    public Type Type => typeof(string);
    public Color Color => Colors.DarkSeaGreen;

    /// <summary>
    ///     Value
    /// </summary>
    public object Value => _fileName;

    public void _SetValue(object? value)
    {
        _fileName = (string?)value ?? "";
    }

    public void Dispose()
    {
    }

    public IControl Control => new FilePathPort(_fileName, _allowExtension);
}