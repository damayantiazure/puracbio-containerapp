using System;
using Xunit;
using Flurl.Http;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public class DurableFunctionExceptionExtensionsTests
{
    private readonly string _exceptionmessage = "My Awesome FlurlHttpException";

    [Fact]
    public async Task MakeDurableFunctionCompatible_ReturnsException_IsSerializable()
    {
        // Arrange 
        var flurlHttpException = new FlurlHttpException(new HttpCall(), _exceptionmessage, new Exception());

        // Act
        var durableException = await flurlHttpException.MakeDurableFunctionCompatible();

        // Verify
        Assert.False(flurlHttpException.GetType().IsSerializable);
        Assert.True(durableException.GetType().IsSerializable);           
    }

    [Fact]
    public async Task MakeDurableFunctionCompatible_ReturnsException_WithSameMessage()
    {
        // Arrange 
        var exceptionMessage = _exceptionmessage;
        var flurlHttpException = new FlurlHttpException(new HttpCall(), exceptionMessage, new Exception());

        // Act
        var durableException = await flurlHttpException.MakeDurableFunctionCompatible();

        // Verify
        Assert.Equal(exceptionMessage, durableException.Message);
    }

    [Fact]
    public async Task MakeDurableFunctionCompatible_ReturnsOrchestrationSessionNotFoundException()
    {
        // Arrange 
        var exceptionMessage = "\"typeKey\":\"OrchestrationSessionNotFoundException\"";
        var flurlHttpException = new FlurlHttpException(new HttpCall(), exceptionMessage, new Exception());

        // Act
        var durableException = await flurlHttpException.MakeDurableFunctionCompatible();

        // Verify
        Assert.Equal("OrchestrationSessionNotFoundException", durableException.GetType().Name);
    }
}