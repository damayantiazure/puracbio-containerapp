#nullable enable

using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests;

public class ImportRegistrationsFunctionTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ICmdbClient> _client;
    private readonly Mock<IPipelineRegistrationMapper> _mapper;
    private readonly Mock<IPipelineRegistrationStorageRepository> _repo;
    private readonly ImportRegistrationsFunction _func;

    public ImportRegistrationsFunctionTests()
    {
        _fixture = new Fixture();
        _client = new Mock<ICmdbClient>();
        _mapper = new Mock<IPipelineRegistrationMapper>();
        _repo = new Mock<IPipelineRegistrationStorageRepository>();
        _func = new ImportRegistrationsFunction(_client.Object, _mapper.Object, _repo.Object);
    }

    [Fact]
    public async Task FunctionExecutesAllRequiredSteps()
    {
        // Arrange
        _client
            .Setup(s => s.GetAzDoCIsAsync())
            .ReturnsAsync(_fixture.CreateMany<CiContentItem>());
        _mapper
            .Setup(s => s.Map(It.IsAny<CiContentItem>()))
            .Returns(_fixture.CreateMany<PipelineRegistration>());
        _repo
            .Setup(s => s.ImportAsync(It.IsAny<IEnumerable<PipelineRegistration>>()));

        // Act
        await _func.RunAsync(null);

        // Assert
        _client.VerifyAll();
        _mapper.VerifyAll();
        _repo.VerifyAll();
    }
}