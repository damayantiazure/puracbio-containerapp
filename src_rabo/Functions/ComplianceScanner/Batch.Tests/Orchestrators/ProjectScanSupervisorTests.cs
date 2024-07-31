#nullable enable

using System;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Orchestrators;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests.Orchestrators;

public class ProjectScanSupervisorTests
{
    private readonly IFixture _fixture = new Fixture();
        
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task RunShouldCallOrchestratorFunctionOnceForEveryProject(int count)
    {
        //Arrange       
        var projects = ProjectsTestHelper.CreateMultipleProjectsResponse(count).ToList();
        var orchestrationClientMock = new Mock<IDurableOrchestrationContext>();
        orchestrationClientMock
            .Setup(x => x.GetInput<(string, List<Response.Project>, DateTime)>())
            .Returns((_fixture.Create<string>(), projects, _fixture.Create<DateTime>()));

        //Act
        var fun = new ProjectScanSupervisor();
        await fun.RunAsync(orchestrationClientMock.Object);

        //Assert
        orchestrationClientMock.Verify(
            x => x.CallSubOrchestratorAsync(
                nameof(ProjectScanOrchestrator), It.IsAny<string>(),
                It.IsAny<(string, Response.Project, DateTime)>()),
            Times.Exactly(count));
    }
}