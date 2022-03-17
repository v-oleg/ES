// ReSharper disable CheckNamespace
namespace ES.Core;

public class Tools
{
    private static Lazy<Tools> _tools = new();
    private static Lazy<Converter> _converter = new();
    
    public static Tools Instance => _tools.Value;

    public Converter Converter => _converter.Value;
}