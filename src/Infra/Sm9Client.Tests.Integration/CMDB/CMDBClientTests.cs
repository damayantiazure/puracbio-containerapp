using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Shouldly;
using System;
using System.Linq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Integration.Cmdb;

[Trait("category", "integration")]
public class CmdbClientTests : IClassFixture<IntegrationTestBootstrapper>
{
    private readonly ICmdbClient _client;
    private const string ReturnCodeSuccess = "0";
    private readonly ConfigurationItem _defaultConfigurationItem;
    private readonly Sm9ClientConfiguration _config = new();

    public CmdbClientTests(IntegrationTestBootstrapper bootstrapper)
    {
        _client = bootstrapper.HostBuilder.Services.GetRequiredService<ICmdbClient>();

        _defaultConfigurationItem = new ConfigurationItem
        {
            CiName = _config.CiName
        };
    }

    [Fact]
    public async Task IntegrationTest_InsertUpdateDeleteDeploymentMethodAsync_WithValidInput_ShouldInsertUpdateAndDeleteDeploymentMethod()
    {
        // Arrange
        var deploymentMethod = new DeploymentMethod(_config.CiName, _config.Organization, _config.Project, _config.Pipeline, "Stage123", _config.Profile);
        var newDeploymentMethod = new DeploymentMethod(_config.CiName, _config.Organization, _config.Project, _config.Pipeline, "Stage456", _config.Profile);
        ManageDeploymentInformationResponse updateResult = null;
        ManageDeploymentInformationResponse deleteResult = null;
        var deploymentMethodListAfterInsert = Enumerable.Empty<DeploymentInformation>();
        var deploymentMethodListAfterUpdate = Enumerable.Empty<DeploymentInformation>();
        var deploymentMethodListAfterDelete = Enumerable.Empty<DeploymentInformation>();

        // Call this synchronous and make sure deployment method does not exist before creating it
        DeleteDeploymentMethodIfExists(deploymentMethod);
        DeleteDeploymentMethodIfExists(newDeploymentMethod);

        // Act

        // Test 1: First insert deployment method, than update this deploymentmethod with new one
        var insertResult = await _client.InsertDeploymentMethodAsync(deploymentMethod);

        // Test 2: Update deployment method with new method
        if (insertResult.ReturnCode == ReturnCodeSuccess)
        {
            deploymentMethodListAfterInsert = await _client.GetDeploymentMethodAsync(_config.CiName);
            var currentMethod = ConvertToSupplementaryInformation(deploymentMethod);
            updateResult = await _client.UpdateDeploymentMethodAsync(_defaultConfigurationItem, currentMethod, newDeploymentMethod);
        }

        // Test 3: Delete created deploymentmethod to prevent 'registration already exist' error in next run
        if (updateResult.ReturnCode == ReturnCodeSuccess)
        {
            deploymentMethodListAfterUpdate = await _client.GetDeploymentMethodAsync(_config.CiName);
            var currentRecord = ConvertToSupplementaryInformation(newDeploymentMethod);
            deleteResult = await _client.DeleteDeploymentMethodAsync(_defaultConfigurationItem, currentRecord);
        }

        if (deleteResult.ReturnCode == ReturnCodeSuccess)
        {
            deploymentMethodListAfterDelete = await _client.GetDeploymentMethodAsync(_config.CiName);
        }

        // Assert
        insertResult.ShouldNotBeNull();
        insertResult.Messages.ShouldContain("The new deployment information was added successfuly!");
        deploymentMethodListAfterInsert.Select(x => x.Information).ShouldContain(deploymentMethod.ToString());
        updateResult.ShouldNotBeNull();
        updateResult.Messages.ShouldContain("The deployment information was updated successfuly");
        deploymentMethodListAfterUpdate.Select(x => x.Information).ShouldNotContain(deploymentMethod.ToString());
        deploymentMethodListAfterUpdate.Select(x => x.Information).ShouldContain(newDeploymentMethod.ToString());
        deleteResult.ShouldNotBeNull();
        deleteResult.Messages.ShouldContain("The deployment information was removed successfuly");
        deploymentMethodListAfterDelete.Select(x => x.Information).ShouldNotContain(newDeploymentMethod.ToString());
    }

    private void DeleteDeploymentMethodIfExists(DeploymentMethod deploymentMethod)
    {
        var existingMethod = ConvertToSupplementaryInformation(deploymentMethod);
        var ci = new ConfigurationItem() { CiName = deploymentMethod.CiName };
        var currentDeploymentMethods = _client.GetDeploymentMethodAsync(_config.CiName).GetAwaiter().GetResult();

        while (currentDeploymentMethods.Select(x => x.Information).Contains(deploymentMethod.ToString()))
        {
            _client.DeleteDeploymentMethodAsync(ci, existingMethod).GetAwaiter().GetResult();
            currentDeploymentMethods = _client.GetDeploymentMethodAsync(_config.CiName).GetAwaiter().GetResult();
        }
    }

    private static SupplementaryInformation ConvertToSupplementaryInformation(DeploymentMethod deploymentMethod)
    {
        return SupplementaryInformation.ParseSupplementaryInfo(deploymentMethod.ToString());
    }
}