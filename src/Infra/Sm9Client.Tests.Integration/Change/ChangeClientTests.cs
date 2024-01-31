using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Integration.Change;

[Trait("category", "integration")]
public class ChangeClientTests : IClassFixture<IntegrationTestBootstrapper>
{
    private readonly Sm9ClientConfiguration _config = new();
    private readonly IChangeClient _client;
    private readonly ISm9ChangesService _sm9ChangesService;

    public ChangeClientTests(IntegrationTestBootstrapper bootstrapper)
    {
        _client = bootstrapper.HostBuilder.Services.GetRequiredService<IChangeClient>();
        _sm9ChangesService = new Sm9ChangesService(_client);
    }

    [Fact]
    public async Task CreateChangeAsync_WithCorrectInput_ShouldCreateChange_And_ShouldCloseChangeAfterwards()
    {
        // Arrange
        var createChangeBody = new CreateChangeRequestBody(_config.Template, _config.Assets)
        {
            Requestor = _config.Requestor,
            Initiator = _config.Initiator,
            JournalUpdate = new[] { _config.JournalUpdate },
            Title = _config.Title,
            Description = _config.Description
        };

        // Act
        var createChange = await _client.CreateChangeAsync(createChangeBody);

        await _sm9ChangesService.ValidateChangesAsync(new[] { createChange.ChangeData.ChangeId },
            new[] { SM9Constants.DeploymentPhase }, 150);

        var closeChangeBody = new CloseChangeRequestBody(createChange.ChangeData.ChangeId)
        {
            ClosureCode = _config.ClosureCode,
            ClosureComments = _config.ClosureComments
        };

        var closeChange = await _client.CloseChangeAsync(closeChangeBody);

        // Assert
        createChange.ChangeData.ChangeId.ShouldNotBeNull();
        createChange.ReturnCode.ShouldBe("0");

        closeChange.ReturnCode.ShouldBe("0");
    }

    [Fact]
    public async Task UpdateChangeAsync_WithApprovalDetails_ShouldUpdateChangeWithApprovalDetails()
    {
        // Arrange
        var requestBody = new UpdateChangeRequestBody(_config.ChangeId)
        {
            ApprovalDetails = new[] { new ApprovalDetails("test.user@rabobank.nl", "pipelineApprover") },
            JournalUpdate = _config.JournalUpdate
        };

        // Act
        var result = await _client.UpdateChangeAsync(requestBody);

        // Assert
        result.ReturnCode.ShouldBe("0");
    }

    [Fact]
    public async Task GetChangeByKeyAsync_WithChangeId_ShouldReturnChangeDetails()
    {
        // Arrange
        var requestBody = new GetChangeByKeyRequestBody(_config.ChangeId);

        // Act
        var result = await _client.GetChangeByKeyAsync(requestBody);

        // Assert
        result!.ReturnCode.ShouldBe("0");
        result.RetrieveChangeInfoByKey!.Information!.Length.ShouldBe(1);
        result.RetrieveChangeInfoByKey.Information[0].Phase.ShouldBe("Closure");
        result.RetrieveChangeInfoByKey.Information[0].Status.ShouldBe("closed");
    }
}