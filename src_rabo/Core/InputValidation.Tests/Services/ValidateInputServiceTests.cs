#nullable enable

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Core.InputValidation.Services;
using System;
using System.Net.Http;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Core.InputValidation.Tests.Services;

public class ValidateInputServiceTests
{
    private const string _organization = "Raboweb";
    private const string _projectId = "1";
    private const string _itemId = "2";
    private const string _ruleName = "RandomRule";

    [Fact]
    public void RequestNotProvided_ReturnsArgumentNullException()
    {
        // Arrange        
        var sut = new ValidateInputService();

        // Act
        var actual = () => sut.Validate(null, _organization, _projectId);

        // Act & Assert
        actual.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, _projectId, _ruleName, _itemId)]
    [InlineData(_organization, null, _ruleName, _itemId)]
    [InlineData(_organization, _projectId, null, _itemId)]
    [InlineData(_organization, _projectId, _ruleName, null)]
    public void ArgumentNotProvided_ReturnsArgumentNullException(string organization, string projectId, string ruleName,
        string itemId)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var sut = new ValidateInputService();

        // Act
        var actual = () => sut.Validate(request, organization, projectId, ruleName, itemId);

        // Assert
        actual.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CiIdentifierNotProvided_ReturnsArgumentNullException()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var sut = new ValidateInputService();

        // Act
        var actual = () => sut.Validate(_organization, _projectId, null, request);

        // Assert
        actual.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateItemType_ItemTypeNotInSpecifiedTypes_ThrowsException()
    {
        // Arrange
        var sut = new ValidateInputService();

        // Act
        var actual = () => sut.ValidateItemType("testType",
            new[] { ItemTypes.ClassicReleasePipeline, ItemTypes.YamlReleasePipeline });

        // Assert
        actual.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ValidateItemType_ItemTypeNotInSpecifiedTypes_NoException()
    {
        // Arrange
        var sut = new ValidateInputService();

        // Act & Assert
        sut.ValidateItemType("classic release",
            new[] { ItemTypes.ClassicReleasePipeline, ItemTypes.YamlReleasePipeline });
    }

    [Fact]
    public void ValidateInputService_ValidateInput_ProjectIdIsNullAsync()
    {
        // Arrange
        const string id = "1";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";
        const bool release = false;
        var sut = new ValidateInputService();

        // Act
        var actual = (BadRequestObjectResult)sut.ValidateInput(null, id, organizationUri, release);

        // Assert
        const string expected = @"An invalid project ID and/or release/run ID was provided.
            ---------------------------------------------------------------------------------------------
            Project IDs should be GUIDs and release/run IDs should be numbers.
            Azure Function syntax:
            - For (YAML-pipeline) Runs: https://validategatesprd.azurewebsites.net/api/validate-yaml-approvers/(System.TeamProjectId)/(Build.BuildId)
            - For (Classic Pipeline) Releases: https://validategatesprd.azurewebsites.net/api/validate-classic-approvers/(System.TeamProjectId)/(Release.ReleaseId)
            Below exception will tell you whether the problem lies with your provided project ID and/or release/run ID.
            ---------------------------------------------------------------------------------------------
            The following exception has been thrown:
            A projectId was not provided in the URL.";

        actual?.Value?.ToString().Should().Be(expected);
    }

    [Fact]
    public void ValidateInputService_ValidateInput_IdIsNullAsync()
    {
        // Arrange
        const string projectId = "1234";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";
        const bool release = false;
        var sut = new ValidateInputService();

        // Act
        var actual = (BadRequestObjectResult)sut.ValidateInput(projectId, null, organizationUri, release);

        // Assert
        const string expected = @"An invalid project ID and/or release/run ID was provided.
            ---------------------------------------------------------------------------------------------
            Project IDs should be GUIDs and release/run IDs should be numbers.
            Azure Function syntax:
            - For (YAML-pipeline) Runs: https://validategatesprd.azurewebsites.net/api/validate-yaml-approvers/(System.TeamProjectId)/(Build.BuildId)
            - For (Classic Pipeline) Releases: https://validategatesprd.azurewebsites.net/api/validate-classic-approvers/(System.TeamProjectId)/(Release.ReleaseId)
            Below exception will tell you whether the problem lies with your provided project ID and/or release/run ID.
            ---------------------------------------------------------------------------------------------
            The following exception has been thrown:
            A runId was not provided in the URL.";

        actual?.Value?.ToString().Should().Be(expected);
    }

    [Fact]
    public void ValidateInputService_ValidateInput_IdIsNotADigit()
    {
        // Arrange
        const string projectId = "1234";
        const string id = "a";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";
        const bool release = false;
        var sut = new ValidateInputService();

        // Act
        var actual = (BadRequestObjectResult)sut.ValidateInput(projectId, id, organizationUri, release);

        // Assert
        const string expected = @"An invalid project ID and/or release/run ID was provided.
            ---------------------------------------------------------------------------------------------
            Project IDs should be GUIDs and release/run IDs should be numbers.
            Azure Function syntax:
            - For (YAML-pipeline) Runs: https://validategatesprd.azurewebsites.net/api/validate-yaml-approvers/(System.TeamProjectId)/(Build.BuildId)
            - For (Classic Pipeline) Releases: https://validategatesprd.azurewebsites.net/api/validate-classic-approvers/(System.TeamProjectId)/(Release.ReleaseId)
            Below exception will tell you whether the problem lies with your provided project ID and/or release/run ID.
            ---------------------------------------------------------------------------------------------
            The following exception has been thrown:
            The runId: 'a' provided in the URL is invalid. It should only consist of numbers.";

        actual?.Value?.ToString().Should().Be(expected);
    }

    [Fact]
    public void ValidateInputService_ValidateInput_OrganizationUriIsEmpty()
    {
        // Arrange
        const string projectId = "1234";
        const string id = "1";
        var organizationUri = string.Empty;
        const bool release = false;
        var sut = new ValidateInputService();

        // Act
        var actual = (BadRequestObjectResult)sut.ValidateInput(projectId, id, organizationUri, release);

        // Assert
        actual?.Value?.ToString().Should().Contain("$(system.CollectionUri)");
    }

    [Fact]
    public void ValidateInputService_ValidateInput_OkObject()
    {
        // Arrange
        const string projectId = "1234";
        const string id = "1";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";
        const bool release = false;
        var sut = new ValidateInputService();

        // Act
        var actual = sut.ValidateInput(projectId, id, organizationUri, release);

        // Assert
        actual.Should().BeOfType(typeof(OkObjectResult));
    }
}