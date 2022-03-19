using System.IO;
using Xunit;

namespace ES.Core.Test.Tools;

public class ResourceToolTests
{
    private string PathToResources = "ES.Core.Test.Tools.";

    [Fact]
    public void Should_Get_Event_From_Resource()
    {
         var @event = ES.Core.Tools.Instance.Converter.ResourceToEvent(GetType().Assembly,
            $"{PathToResources}PersonCreated.json");
         
         Assert.Equal("PersonCreated", @event.EventName);
    }
    
    [Fact]
    public void Should_Not_Get_Event_If_Resource_Not_Found()
    {
        Assert.Throws<FileNotFoundException>(() => ES.Core.Tools.Instance.Converter.ResourceToEvent(
            GetType().Assembly, $"{PathToResources}PersonCreated1.json"));
    }
}