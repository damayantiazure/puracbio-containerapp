using AutoFixture;
using NSubstitute;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using Task = System.Threading.Tasks.Task;
using Rabobank.Compliancy.Core.Rules.Rules;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class YamlReleasePipelineHasRequiredRetentionPolicyTests
{
    [Theory]
    [InlineData(30, false)]
    [InlineData(450, true)]
    [InlineData(null, false)]
    public async Task ShouldCheckIfProjectHasRequiredRetentionPolicy(int? purgeRuns,
        bool isCompliant)
    {
        //Arrange
        var fixture = new Fixture();
        var buildPipeline = fixture.Create<BuildDefinition>();
        var organization = fixture.Create<string>();
        var projectId = fixture.Create<string>();

        var retentionSettings = fixture.Create<ProjectRetentionSetting>();
        retentionSettings.PurgeRuns = purgeRuns.HasValue ? new RetentionSetting() { Value = purgeRuns.Value } : null;
        var client = Substitute.For<IAzdoRestClient>();
        client
            .GetAsync(Arg.Any<IAzdoRequest<ProjectRetentionSetting>>(), organization)
            .Returns(retentionSettings);

        //Act
        var rule = new YamlReleasePipelineHasRequiredRetentionPolicy(client);
        var result = await rule.EvaluateAsync(organization, projectId, buildPipeline);

        //Assert
        result.ShouldBe(isCompliant);
    }
}