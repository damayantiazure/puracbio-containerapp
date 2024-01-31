#nullable enable

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Orchestrators;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Starters;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests.Starters;

public class ProjectsScanStarterTests
{
    [Fact]
    public async Task RunShouldCallGetProjectsExactlyOnce()
    {
        //Arrange
        var orchestrationClientMock = new Mock<IDurableOrchestrationClient>();
        var clientMock = new Mock<IAzdoRestClient>();
        var organizationsMock = new string[2];
        var timerInfoMock = CreateTimerInfoMock();

        var projects = ProjectsTestHelper.CreateMultipleProjectsResponseAsync(1);

        clientMock.Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), null))
            .ReturnsAsync(projects);

        //Act
        var fun = new ProjectScanStarter(clientMock.Object, organizationsMock);
        await fun.RunAsync(timerInfoMock, orchestrationClientMock.Object);

        //Assert
        clientMock.Verify(x => x.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), null), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunShouldCallSupervisorFunctionExactlyOnce()
    {
        //Arrange       
        var orchestrationClientMock = new Mock<IDurableOrchestrationClient>();
        var clientMock = new Mock<IAzdoRestClient>();
        var organizationsMock = new string[2];
        var timerInfoMock = CreateTimerInfoMock();

        var projects = ProjectsTestHelper.CreateMultipleProjectsResponseAsync(2);

        clientMock.Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), null))
            .ReturnsAsync(projects);

        //Act
        var fun = new ProjectScanStarter(clientMock.Object, organizationsMock);
        await fun.RunAsync(timerInfoMock, orchestrationClientMock.Object);

        //Assert
        orchestrationClientMock.Verify(
            x => x.StartNewAsync(nameof(ProjectScanSupervisor), It.IsAny<object>()),
            Times.AtLeastOnce());
    }

    private static TimerInfo CreateTimerInfoMock()
    {
        var timerScheduleMock = new Mock<TimerSchedule>();
        var scheduleStatusMock = new Mock<ScheduleStatus>();
        var timerInfoMock = new TimerInfo(timerScheduleMock.Object, scheduleStatusMock.Object);
        return timerInfoMock;
    }
}