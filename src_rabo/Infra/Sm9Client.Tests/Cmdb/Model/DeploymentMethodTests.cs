using Rabobank.Compliancy.Domain.RuleProfiles;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Cmdb.Model;

public class DeploymentMethodTests
{
    [Fact]
    public void ToString_FieldsFilled_ReturnsCorrectFormat()
    {
        // Arrange
        const string organization = "raboweb";
        const string projectId = "f64ffdfa-0c4e-40d9-980d-bb8479366fc5";
        const string ciName = "Unitttest";
        const string pipelineId = "1234";
        const string stageId = "prod";
        const string profile = nameof(Profiles.Default);

        var update = new Sm9Client.Cmdb.Model.DeploymentMethod(ciName, organization, projectId, pipelineId, stageId, profile);

        // Act
        var result = update.ToString();

        // Assert
        result.Should().Be($"{{\"organization\":\"{organization}\",\"project\":\"{projectId}\",\"pipeline\":\"{pipelineId}\",\"stage\":\"{stageId}\", \"profile\":\"{profile}\"}}");
    }
}