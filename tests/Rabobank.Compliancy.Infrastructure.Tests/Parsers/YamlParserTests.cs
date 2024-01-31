using Rabobank.Compliancy.Infrastructure.Models.Yaml;
using Rabobank.Compliancy.Infrastructure.Parsers;
using Rabobank.Compliancy.Infrastructure.Tests.Helpers;
using YamlDotNet.Core;
using Environment = Rabobank.Compliancy.Infrastructure.Models.Yaml.Environment;

namespace Rabobank.Compliancy.Infrastructure.Tests.Parsers;

public class YamlParserTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void ParseToYamlModel_ValidYamlFile_ShouldNotReturnEmptyInstance()
    {
        // Arrange
        var content = ResourceFileHelper.GetContentFromResourceFile("buildpipeline.yml");

        // Act
        var actual = YamlParser.ParseToYamlModel(content);

        // Assert
        actual.Should().NotBeNull();
    }

    [Fact]
    public void ParseToYamlModel_NoYamlContent_ShouldReturnNull()
    {
        // Arrange
        string? content = null;

        // Act
        var actual = YamlParser.ParseToYamlModel(content);

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public void ParseToYamlModel_InvalidYamlFile_ShouldThrowException()
    {
        // Arrange
        var content = _fixture.Create<string>();

        // Act
        Func<YamlModel> actual = () => YamlParser.ParseToYamlModel(content);

        // Assert
        actual.Should().Throw<YamlException>();
    }

    [Fact]
    public void ParseToYamlModelYamlWithDownloadTask_ShouldReturnCorrectModel()
    {
        // Arrange
        var content = ResourceFileHelper.GetContentFromResourceFile("PipelineWithStagesAndDownloadTask.yml");

        // Act
        var actual = YamlParser.ParseToYamlModel(content);

        // Assert
        actual.Should().NotBeNull();
        actual.Stages.Should().BeEquivalentTo(new List<StageModel>
        {
            new StageModel
            {
                Stage = "stage1",
                Jobs = new List<JobModel>
                {
                    new JobModel
                    {
                        Job = null,
                        Deployment = "Test Deploy",
                        Environment = new Environment
                        {
                            Name = "testprod"
                        },
                        Strategy = new Strategy {
                            RunOnce =  new RunOnce
                            {
                                Deploy = new Deploy
                                {
                                    Steps = new List<StepModel>
                                    {
                                        new StepModel
                                        {
                                            Task = "DownloadPipelineArtifact@2",
                                            DisplayName ="Download Artifact from outside project",
                                            Inputs = new Dictionary<string, string>
                                            {
                                                { "project", "106f7e65-65cc-45a1-980f-a90e414ec820" },
                                                { "definition", "1258" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });
    }

    [Fact]
    public void ParseToYamlModel_YamlWithStrategy_ShouldReturnCorrectModel()
    {
        // Arrange
        var content = ResourceFileHelper.GetContentFromResourceFile("PipelineWithStagesAndDownloadTask.yml");

        // Act
        var actual = YamlParser.ParseToYamlModel(content);

        // Assert
        actual.Should().NotBeNull();
        actual.Stages.SelectMany(s => s.Jobs.SelectMany(j => j.GetAllSteps())).Should().BeEquivalentTo(new List<StepModel>
        {
            new StepModel
            {
                Task = "DownloadPipelineArtifact@2",
                DisplayName ="Download Artifact from outside project",
                Inputs = new Dictionary<string, string>
                {
                    { "project", "106f7e65-65cc-45a1-980f-a90e414ec820" },
                    { "definition", "1258" }
                }
            }
        });
    }
}