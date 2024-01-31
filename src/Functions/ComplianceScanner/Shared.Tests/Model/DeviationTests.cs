using System;
using AutoFixture;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Deviations;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Model;

public class DeviationTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void CreateRowKey_ShouldReturnProperRowKey()
    {
        // arrange
        const string organization = "organizationName";
        const string projectId = "projectId";
        const string ruleName = "ruleName";
        const string itemId = "itemId";
        const string ciIdentifier = "CI1234567";
        const string foreignProjectId = "foreignProjectId";

        // act
        var actual = Deviation.CreateRowKey(organization, projectId, ruleName, itemId, ciIdentifier, foreignProjectId);

        // assert
        actual.ShouldBe("b967f6452ff06afc4d8b0ed84a71104d");
    }

    /// <summary>
    ///     The Shared.Model.Deviation is being refactored into DeviationEntity but for the time being
    ///     both the Shared.Model.Deviation and DeviationEntity will be in use meaning the rowKeys should be
    ///     equal so they can operate on the same data.
    /// </summary>
    [Fact]
    public void DeviationEntityRowKey_WhenAllParametersPresent_ShouldMatchEquivalentDeviationRowKey()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var itemId = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        Guid? foreignProjectId = _fixture.Create<Guid>();

        var deviation = new Deviation(organization, projectName, ruleName,
            itemId, ciIdentifier, projectId.ToString(), foreignProjectId.ToString());

        var deviationEntity = new DeviationEntity(deviation.Organization, deviation.ProjectName, deviation.RuleName,
            deviation.ItemId, deviation.CiIdentifier, projectId, deviation.Comment, deviation.Reason,
            deviation.UpdatedBy, foreignProjectId);

        // Act & Assert
        deviationEntity.RowKey.ShouldBeEquivalentTo(deviation.RowKey);
    }

    /// <summary>
    ///     The Shared.Model.Deviation is being refactored into DeviationEntity but for the time being
    ///     both the Shared.Model.Deviation and DeviationEntity will be in use meaning the rowKeys should be
    ///     equal so they can operate on the same data.
    /// </summary>
    [Fact]
    public void DeviationEntityRowKey_WhenNullableParametersAbsent_ShouldMatchEquivalentDeviationRowKey()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var itemId = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();

        var deviation = new Deviation(organization, projectName, ruleName,
            itemId, ciIdentifier, projectId.ToString(), null);

        var deviationEntity = new DeviationEntity(deviation.Organization, deviation.ProjectName, deviation.RuleName,
            deviation.ItemId, deviation.CiIdentifier, projectId, deviation.Comment, deviation.Reason,
            deviation.UpdatedBy, null);

        // Act & Assert
        deviationEntity.RowKey.ShouldBe(deviation.RowKey);
    }
}