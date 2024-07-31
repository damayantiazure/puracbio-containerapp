#nullable enable

using Shouldly;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Helpers.OrchestrationHelper;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests.Helpers;

public class OrchestrationIdHelperTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void GetSupervisorIdShouldReturnNullWhenNoSupervisorId()
    {
        //Arrange
        var instanceId = _fixture.Create<string>();

        //Act
        var result = GetSuperVisorIdForScopeOrchestrator(instanceId);

        //Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ProjectScanOrchestrationIdShouldReturnSupervisorId()
    {
        //Arrange
        var supervisorId = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var instanceId = CreateProjectScanOrchestrationId(supervisorId, projectId);

        //Act
        var result = GetSuperVisorIdForProjectOrchestrator(instanceId);

        //Assert
        result.ShouldBe(supervisorId);
    }
}