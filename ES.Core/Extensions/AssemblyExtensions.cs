using System.Reflection;

namespace ES.Core.Extensions;

public static class AssemblyExtensions
{
    public static string? ResourceAsString(this Assembly assembly, string resourcePath)
    {
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
            return null;
        
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
}