using System;
using AutoFixture;
using NSubstitute;
using Shouldly;
using Xunit;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public class AzdoRestRequestExtensionsTests 
{
    [Fact]
    public void RestRequestAsJson()
    {
        var fixture = new Fixture();
        var request = Substitute.For<IAzdoRequest<int>>();
            
        var uri = fixture.Create<string>();
        request.Resource.Returns(uri);

        var baseUri = fixture.Create<Uri>();
        request.BaseUri(Arg.Any<string>()).Returns(baseUri);

        var target = request.AsJson();
        target.Resource.ShouldBe(uri);
        target.BaseUri(fixture.Create<string>()).ShouldBe(baseUri);
    }
}