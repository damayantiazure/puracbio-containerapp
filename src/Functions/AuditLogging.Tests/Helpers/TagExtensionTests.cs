#nullable enable

using Rabobank.Compliancy.Functions.AuditLogging.Helpers;
using Shouldly;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests.Helpers;

public class TagExtensionTests
{
    private readonly Fixture _fixture;

    public TagExtensionTests() => _fixture = new Fixture();
        
    [Theory]
    [InlineData("C000123456 [00aa0a00]", true)]
    [InlineData("Random tag", false)]
    [InlineData("C000123456", true)]
    [InlineData("T000123456", false)]
    public void ShouldValidateIfTagIsChangeTag(string tag, bool expectedResult)
    {
        //Act
        var result = tag.IsChangeTag();

        //Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("C000123456 [00aa0a00]", "C000123456")]
    [InlineData("Random tag", "")]
    [InlineData("C000123456", "C000123456")]
    public void ShouldRetrieveChangeIdFromTag(string tag, string expectedResult)
    {
        //Act
        var result = tag.ChangeId();

        //Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("C000123456 [00aa0a00]", "http://itsm.rabobank.nl/SM/index.do?" +
                                         "ctx=docEngine&file=cm3r&query=number%3D%22C000123456%22&action=&" +
                                         "title=Change%20Request%20Details&queryHash=00aa0a00")]
    [InlineData("Random tag", "")]
    [InlineData("C000123456", "")]
    public void ShouldRetrieveChangeUrlFromTag(string tag, string expectedResult)
    {
        //Act
        var result = tag.ChangeUrl();

        //Assert
        result?.AbsoluteUri.ShouldBe(expectedResult);
    }
}