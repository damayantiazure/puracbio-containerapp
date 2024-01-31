using AutoFixture;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests;

public class PipelineRegistrationExtensionsTest
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void GetCiIdentifiersDisplayString_OneCiId_ReturnsStringWithOneCiId()
    {
        // Arrange
        const string CiId = "CI1";
        var registration1 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, CiId)
            .Create();
            
        var registrations = new List<PipelineRegistration> { registration1 };

        // Act
        var result = registrations.GetCiIdentifiersDisplayString();

        // Assert
        result.ShouldBe(CiId);
    }

    [Fact]
    public void GetCiIdentifiersDisplayString_SameCiIdTwice_ReturnsStringWithOneCiId()
    {
        // Arrange
        string[] ciIds = { "CI1", "CI1" };
        var registration1 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, ciIds[0])
            .Create();
        var registration2 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, ciIds[1])
            .Create();

        var registrations = new List<PipelineRegistration> { registration1 };

        // Act
        var result = registrations.GetCiIdentifiersDisplayString();

        // Assert
        result.ShouldBe(ciIds[0]);
    }

    [Fact]
    public void GetCiIdentifiersDisplayString_ThreeCiIds_ReturnsStringWithThreeCiIds()
    {
        // Arrange
        string[] ciIds ={ "CI1", "CI2", "CI3" };
        var registration1 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, ciIds[0])
            .Create();
        var registration2 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, ciIds[1])
            .Create();
        var registration3 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, ciIds[2])
            .Create();

        var registrations = new List<PipelineRegistration> { registration1, registration2, registration3 };

        // Act
        var result = registrations.GetCiIdentifiersDisplayString();

        // Assert
        result.ShouldBe($"{ciIds[0]}, {ciIds[1]}, {ciIds[2]}");
    }

    [Fact]
    public void GetCiNamesDisplayString_OneCiName_ReturnsStringWithOneCiName()
    {
        // Arrange
        const string ciName = "Azdo";
        var registration1 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiName, ciName)
            .Create();

        var registrations = new List<PipelineRegistration> { registration1 };

        // Act
        var result = registrations.GetCiNamesDisplayString();

        // Assert
        result.ShouldBe(ciName);
    }

    [Fact]
    public void GetCiIdentifiersDisplayString_SameCiNameTwice_ReturnsStringWithOneCiName()
    {
        // Arrange
        string[] ciNames = { "Azdo1", "Azdo1" };
        var registration1 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, ciNames[0])
            .Create();
        var registration2 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiIdentifier, ciNames[1])
            .Create();

        var registrations = new List<PipelineRegistration> { registration1, registration2 };

        // Act
        var result = registrations.GetCiIdentifiersDisplayString();

        // Assert
        result.ShouldBe(ciNames[0]);
    }

    [Fact]
    public void GetCiNamesDisplayString_ThreeCiNames_ReturnsStringWithThreeCiNames()
    {
        // Arrange
        string[] ciNames = { "Azdo1", "Azdo2", "Azdo3" };
        var registration1 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiName, ciNames[0])
            .Create();
        var registration2 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiName, ciNames[1])
            .Create();
        var registration3 = _fixture.Build<PipelineRegistration>()
            .With(r => r.CiName, ciNames[2])
            .Create();

        var registrations = new List<PipelineRegistration> { registration1, registration2, registration3 };

        // Act
        var result = registrations.GetCiNamesDisplayString();

        // Assert
        result.ShouldBe($"{ciNames[0]}, {ciNames[1]}, {ciNames[2]}");
    }
}