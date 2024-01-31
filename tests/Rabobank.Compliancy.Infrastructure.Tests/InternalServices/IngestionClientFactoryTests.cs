using AutoFixture.AutoMoq;
using Azure.Core;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.InternalServices;

namespace Rabobank.Compliancy.Infrastructure.Tests.InternalServices;

public class IngestionClientFactoryTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly TokenCredential _tokenCredential;

    public IngestionClientFactoryTests() =>
        _tokenCredential = _fixture.Create<TokenCredential>();

    [Fact]
    public void Create_FirstCall_ShouldReturnNewInstance()
    {
        // Arrange
        var modelName = _fixture.Create<string>();

        var logIngestionConfig = CreateLogIngestionConfig(modelName);

        var sut = CreateSut(logIngestionConfig);

        // Act
        var clientFactoryResult = sut.Create(modelName);

        // Assert
        clientFactoryResult.Client.Should().NotBeNull();
        clientFactoryResult.ClientConfig.Should().Be(logIngestionConfig.Clients.Single());
    }

    [Fact]
    public void Create_SecondCall_ShouldReturnInstanceFromCache()
    {
        // Arrange
        var modelName = _fixture.Create<string>();

        var logIngestionConfig = CreateLogIngestionConfig(modelName);

        var sut = CreateSut(logIngestionConfig);

        // Act
        var clientFactoryResultOne = sut.Create(modelName);
        var clientFactoryResultTwo = sut.Create(modelName);

        // Assert
        clientFactoryResultOne.Client.Should().Be(clientFactoryResultTwo.Client);
    }

    private LogIngestionConfig CreateLogIngestionConfig(string modelName)
    {
        var logIngestionClientConfigs = _fixture
            .Build<LogIngestionClientConfig>()
            .With(f => f.ModelName, modelName)
            .CreateMany(1);

        return _fixture
            .Build<LogIngestionConfig>()
            .With(f => f.Clients, logIngestionClientConfigs)
            .Create();
    }

    private IIngestionClientFactory CreateSut(LogIngestionConfig logIngestionConfig) =>
        new IngestionClientFactory(_tokenCredential, logIngestionConfig);
}