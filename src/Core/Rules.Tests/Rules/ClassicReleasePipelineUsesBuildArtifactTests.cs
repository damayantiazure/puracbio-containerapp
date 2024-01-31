using AutoFixture;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class ClassicReleasePipelineUsesBuildArtifactTests
{
    [Fact]
    public async Task ReturnFalseForReleasePipelineWithoutArtifacts()
    {
        //Arrange
        var fixture = new Fixture();
        var releasePipeline = fixture.Create<ReleaseDefinition>();
        releasePipeline.Artifacts = new List<Artifact>();

        //Act
        var rule = new Core.Rules.Rules.ClassicReleasePipelineUsesBuildArtifact(null);
        var result = await rule.EvaluateAsync(fixture.Create<string>(), fixture.Create<string>(), releasePipeline);

        //Assert
        result.ShouldBe(false);
    }
}