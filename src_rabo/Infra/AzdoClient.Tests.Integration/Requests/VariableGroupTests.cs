using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

public class VariableGroupTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public VariableGroupTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Organization, _config.Token);
    }

    [Fact]
    public async Task VariableGroupAddUpdateDelete()
    {
        var groupName = $"TestVariableGroup{Guid.NewGuid()}";
        var updatedGroupName = $"TestVariableGroupUpdated{Guid.NewGuid()}";

        // Create Variable Group
        var newVariableGroupBody = new Response.VariableGroup
        {
            Type = "Vsts",
            Name = groupName,
            Description = "A test variable group",
            Variables = new Dictionary<string, Response.VariableValue>
            {
                { "value1", new Response.VariableValue { IsSecret = true, Value = "test" } }
            }
        };

        var addVariableGroup = await _client.PostAsync(VariableGroup.VariableGroups(
            _config.ProjectId), newVariableGroupBody);

        addVariableGroup.ShouldNotBeNull();
        addVariableGroup.Name.ShouldBe(groupName);
        addVariableGroup.Description.ShouldNotBeEmpty();
        addVariableGroup.Variables.ShouldNotBeNull();

        // Update Variable Group
        var updateVariableGroupBody = new Response.VariableGroup
        {
            Description = "Changed",
            Name = updatedGroupName,
            Type = addVariableGroup.Type,
            Variables = addVariableGroup.Variables
        };

        var updateVariableGroup = await _client.PutAsync(VariableGroup.VariableGroupWithId(
            _config.ProjectId, addVariableGroup.Id), updateVariableGroupBody);

        updateVariableGroup.ShouldNotBeNull();
        updateVariableGroup.Name.ShouldBe(updatedGroupName);
        updateVariableGroup.Description.ShouldBe("Changed");


        // Remove Variable Group
        await _client.DeleteAsync(VariableGroup.VariableGroupWithId(
            _config.ProjectId, addVariableGroup.Id));

        var deletedVariableGroup = await _client.GetAsync(VariableGroup.VariableGroupWithId(
            _config.ProjectId, addVariableGroup.Id));

        deletedVariableGroup.ShouldBeNull();
    }
}