#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Newtonsoft.Json;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class ExclusionStorageRepositoryTests
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IStorageRepository> _storageRepositoryMock = new();

    [Fact]
    public async Task ExclusionExistsAsync_ExclusionFound_ReturnsExclusion()
    {
        // Arrange
        var runInfo = _fixture.Create<PipelineRunInfo>();

        var tableResult = new TableResult
        {
            Result = _fixture.Create<Exclusion>()
        };

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<Exclusion>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(tableResult)
            .Verifiable();

        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act
        var actual = await sut.GetExclusionAsync(runInfo);

        // Assert
        actual.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExclusionExistsAsync_ExclusionNotFound_ReturnsNull()
    {
        // Arrange
        var runInfo = _fixture.Create<PipelineRunInfo>();

        var cloudTableMock = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), null);
        cloudTableMock
            .Setup(m => m.ExecuteAsync(It.IsAny<TableOperation>()))
            .ReturnsAsync((TableResult?)null)
            .Verifiable();

        _storageRepositoryMock
            .Setup(m => m.CreateTable(It.IsAny<string>()))
            .Returns(cloudTableMock.Object);

        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act
        var actual = await sut.GetExclusionAsync(runInfo);

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public async Task CreateExclusionAsync_CanCreateExclusionObjectAndUploadToTableStorageAsync()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                    _fixture.Build<ExclusionReport>()
                        .With(e => e.Reason, "Because")
                        .Create()),
                Encoding.UTF8, "application/json")
        };

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var userMail = _fixture.Create<string>();

        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(e => e.Organization, organization)
            .With(e => e.ProjectId, projectId)
            .With(e => e.PipelineId, pipelineId)
            .With(e => e.PipelineType, pipelineType)
            .Create();

        _fixture.Customize<User>(customizationComposer => customizationComposer
            .With(user => user.MailAddress, userMail));
        var user = _fixture.Create<User>();

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetInteractiveUserAsync(request, organization))
            .ReturnsAsync(user)
            .Verifiable();

        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act
        var actual = await sut.CreateExclusionAsync(request, runInfo);

        // Assert
        actual.ShouldBeOfType(typeof(OkObjectResult));
        _storageRepositoryMock
            .Verify(storageRepository => storageRepository.InsertOrReplaceAsync(It.Is<List<Exclusion>>(x =>
                x[0].Organization == organization &&
                x[0].ProjectId == projectId &&
                x[0].PipelineId == pipelineId &&
                x[0].PipelineType == pipelineType &&
                x[0].ExclusionReasonRequester == "Because" &&
                x[0].Requester == user.MailAddress &&
                x[0].ExclusionReasonApprover == null &&
                x[0].Approver == null &&
                x[0].RunId == null)), Times.Once);
    }

    [Fact]
    public async Task ShouldThrowErrorWhenReasonIsEmpty()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                    _fixture.Build<ExclusionReport>()
                        .With(e => e.Reason, "")
                        .Create()),
                Encoding.UTF8, "application/json")
        };

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var userMail = _fixture.Create<string>();

        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(e => e.Organization, organization)
            .With(e => e.ProjectId, projectId)
            .With(e => e.PipelineId, pipelineId)
            .With(e => e.PipelineType, pipelineType)
            .Create();

        _fixture.Customize<User>(x => x
            .With(user => user.MailAddress, userMail));
        var user = _fixture.Create<User>();

        _authorizationServiceMock
            .Setup(x => x.GetInteractiveUserAsync(request, organization))
            .ReturnsAsync(user)
            .Verifiable();

        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act
        var actual = await sut.CreateExclusionAsync(request, runInfo);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
        ((ObjectResult)actual).Value!.ToString().ShouldStartWith("No valid reason provided.");
        _storageRepositoryMock
            .Verify(x => x.InsertOrMergeAsync(It.IsAny<Exclusion>()), Times.Never);
    }

    [Fact]
    public async Task CanUpdateExclusionObjectAsync()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                    _fixture.Build<ExclusionReport>()
                        .With(e => e.Reason, "Because")
                        .Create()),
                Encoding.UTF8, "application/json")
        };

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        const string approver = "approver";

        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(e => e.Organization, organization)
            .With(e => e.ProjectId, projectId)
            .With(e => e.PipelineId, pipelineId)
            .With(e => e.PipelineType, pipelineType)
            .Create();

        _fixture.Customize<User>(x => x
            .With(user => user.MailAddress, approver));
        var user = _fixture.Create<User>();

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetInteractiveUserAsync(request, organization))
            .ReturnsAsync(user)
            .Verifiable();

        var tableResult = new TableResult
        {
            Result = _fixture.Build<Exclusion>()
                .Without(e => e.Approver)
                .Without(e => e.ExclusionReasonApprover)
                .Create()
        };

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<Exclusion>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(tableResult)
            .Verifiable();

        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act
        var actual = await sut.UpdateExclusionAsync(request, runInfo);

        // Assert
        actual.ShouldBeOfType(typeof(OkObjectResult));
        _storageRepositoryMock
            .Verify(storageRepository => storageRepository.InsertOrMergeAsync(It.Is<Exclusion>(x =>
                x.Approver == approver &&
                x.ExclusionReasonApprover == "Because")), Times.Once);
    }

    [Fact]
    public async Task ShouldThrowErrorForInvalidApprover()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                    _fixture.Build<ExclusionReport>()
                        .With(e => e.Reason, "Because")
                        .Create()),
                Encoding.UTF8, "application/json")
        };

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        const string approver = "requester";

        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(e => e.Organization, organization)
            .With(e => e.ProjectId, projectId)
            .With(e => e.PipelineId, pipelineId)
            .With(e => e.PipelineType, pipelineType)
            .Create();

        _fixture.Customize<User>(x => x
            .With(user => user.MailAddress, approver));
        var user = _fixture.Create<User>();

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetInteractiveUserAsync(request, organization))
            .ReturnsAsync(user)
            .Verifiable();

        var tableResult = new TableResult
        {
            Result = _fixture.Build<Exclusion>()
                .With(e => e.Requester, "requester")
                .Without(e => e.Approver)
                .Without(e => e.ExclusionReasonApprover)
                .Create()
        };

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<Exclusion>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(tableResult)
            .Verifiable();

        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act
        var actual = await sut.UpdateExclusionAsync(request, runInfo);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
        ((ObjectResult)actual).Value!.ToString().ShouldStartWith("The approval of your exclusion request failed");
        _storageRepositoryMock
            .Verify(storageRepository => storageRepository.InsertOrMergeAsync(It.IsAny<Exclusion>()), Times.Never);
    }

    [Fact]
    public async Task ShouldThrowErrorIfRequestIsAlreadyApproved()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                    _fixture.Build<ExclusionReport>()
                        .With(e => e.Reason, "Because")
                        .Create()),
                Encoding.UTF8, "application/json")
        };

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        const string approver = "2ndApprover";

        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(e => e.Organization, organization)
            .With(e => e.ProjectId, projectId)
            .With(e => e.PipelineId, pipelineId)
            .With(e => e.PipelineType, pipelineType)
            .Create();

        _fixture.Customize<User>(x => x
            .With(user => user.MailAddress, approver));
        var user = _fixture.Create<User>();

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetInteractiveUserAsync(request, organization))
            .ReturnsAsync(user)
            .Verifiable();

        var tableResult = new TableResult
        {
            Result = _fixture.Build<Exclusion>()
                .With(e => e.Approver)
                .Create()
        };

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<Exclusion>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(tableResult)
            .Verifiable();

        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act
        var actual = await sut.UpdateExclusionAsync(request, runInfo);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
        ((ObjectResult)actual).Value!.ToString()
            .ShouldStartWith("There already is a valid exclusion for this pipeline.");
        _storageRepositoryMock
            .Verify(storageRepository => storageRepository.InsertOrMergeAsync(It.IsAny<Exclusion>()), Times.Never);
    }

    [Fact]
    public async Task SetRunId_PipelineInfoNull_ThrowsException()
    {
        // Arrange
        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SetRunIdAsync(null));
    }

    [Fact]
    public async Task SetRunId_PipelineInfoWithRunIdExclusionNull_RunIdUpdated()
    {
        // Arrange
        const string runId = "123";
        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);
        var pipelineRunInfo = new PipelineRunInfo("", "", "", "") { RunId = runId };

        var tableResultNull = new TableResult
        {
            Result = null
        };

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<Exclusion>("Exclusion", It.IsAny<string>()))
            .ReturnsAsync(tableResultNull);

        // Act
        await sut.SetRunIdAsync(pipelineRunInfo);

        // Assert
        _storageRepositoryMock.Verify(m => m.InsertOrMergeAsync(It.IsAny<Exclusion>()), Times.Never);
    }

    [Fact]
    public async Task SetRunId_PipelineInfoWithRunId_RunIdUpdated()
    {
        // Arrange
        const string runId = "123";
        var sut = new ExclusionStorageRepository(_storageRepositoryMock.Object, _authorizationServiceMock.Object);
        var pipelineRunInfo = new PipelineRunInfo("", "", "", "") { RunId = runId };

        var tableResult = new TableResult
        {
            Result = _fixture.Build<Exclusion>()
                .With(e => e.RunId, runId)
                .Create()
        };

        _storageRepositoryMock
            .Setup(m => m.GetEntityAsync<Exclusion>("Exclusion", It.IsAny<string>()))
            .ReturnsAsync(tableResult);

        // Act
        await sut.SetRunIdAsync(pipelineRunInfo);

        // Assert
        _storageRepositoryMock.Verify(m => m.InsertOrMergeAsync(It.Is<Exclusion>(e => e.RunId == runId)), Times.Once);
    }
}