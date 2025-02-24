using System.IO;
using System.Reflection;

namespace NodeVideoEffects.Utility;

public static class ResourceLoader
{
    public static string FileLoad(string fileName)
    {
        var asm = Assembly.GetCallingAssembly();
        var resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith(fileName));
        if (resName == null)
            return string.Empty;
        using var st = asm.GetManifestResourceStream(resName);
        if (st == null) return string.Empty;
        var reader = new StreamReader(st);
        return reader.ReadToEnd().Trim('\r', '\n');
    }
}