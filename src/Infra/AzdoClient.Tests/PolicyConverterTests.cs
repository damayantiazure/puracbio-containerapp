using System.IO;
using Newtonsoft.Json;
using Rabobank.Compliancy.Infra.AzdoClient.Converters;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public class PolicyConverterTests
{
    [Fact]
    public void MinimumNumberOfReviewersPolicyConvertTest()
    {
        var input = File.ReadAllText(Path.Combine("Assets/", "MinimumNumberOfReviewers.json"));
        var result = JsonConvert.DeserializeObject<Policy>(input, new PolicyConverter());

        result.ShouldBeOfType<MinimumNumberOfReviewersPolicy>();
    }
        
    [Fact]
    public void RequiredReviewersPolicyConvertTest()
    {
        var input = File.ReadAllText(Path.Combine("Assets", "RequiredReviewers.json"));
        var result = JsonConvert.DeserializeObject<Policy>(input, new PolicyConverter());

        result.ShouldBeOfType<RequiredReviewersPolicy>();
    }
}