using Xunit;

namespace ES.Core.Test.Tools;

public class GuidToolsTests
{
    [Fact]
    public void Should_Return_Same_Guid_For_Same_String()
    {
        var str = "some random string";

        var res = ES.Core.Tools.Instance.Converter.StringToGuid(str);
        var res1 = ES.Core.Tools.Instance.Converter.StringToGuid(str);
        
        Assert.Equal(res, res1);
    }
    
    [Fact]
    public void Should_Return_Different_Guid_For_Different_String()
    {
        var str = "some random string";
        var str1 = "some random string1";

        var res = ES.Core.Tools.Instance.Converter.StringToGuid(str);
        var res1 = ES.Core.Tools.Instance.Converter.StringToGuid(str1);
        
        Assert.NotEqual(res, res1);
    }
}