using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Tests.Resources;

public static class PipelineYaml
{
    public static readonly string FirstProdStage = "Stage1";
    public static readonly string SecondProdStage = "Stage2";
    public static readonly EnvironmentYaml FirstEnvironment = CreateEnvironment(1);
    public static readonly EnvironmentYaml SecondEnvironment = CreateEnvironment(2);

    public static EnvironmentYaml CreateEnvironment(int id)
    {
        var environmentYaml = new EnvironmentYaml
        {
            Id = id,
            Name = $"Environment{id}"
        };

        return environmentYaml;
    }

    public static readonly string SingleStageSingleEnvironment = @$"stages:
- stage: {FirstProdStage}
  jobs:
  - deployment: DeployWeb
    environment: {FirstEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
";

    public static readonly string SingleStageSingleEnvironmentWithAlternativeNames = @$"stages:
- stage:
    name: {FirstProdStage}
  jobs:
  - deployment: DeployWeb
    environment:
      name: {FirstEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
";

    public static readonly string TwoProdStagesFirstWithSecondWithoutEnvironment = @$"stages:
- stage: {FirstProdStage}
  jobs:
  - deployment: DeployWeb1
    environment: {FirstEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
- stage: {SecondProdStage}
  jobs:
  - job: DeployWeb2
    steps:
    - script: echo Hello World!
";

    public static readonly string TwoProdStagesWithSameEnvironment = @$"stages:
- stage: {FirstProdStage}
  jobs:
  - deployment: DeployWeb1
    environment: {FirstEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
- stage: {SecondProdStage}
  jobs:
  - deployment: DeployWeb2
    environment: {FirstEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
";

    public static readonly string TwoProdStagesWithDifferentEnvironment = @$"stages:
- stage: {FirstProdStage}
  jobs:
  - deployment: DeployWeb1
    environment: {FirstEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
- stage: {SecondProdStage}
  jobs:
  - deployment: DeployWeb2
    environment: {SecondEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
";

    public static readonly string SingleProdStagesWithTwoEnvironments = @$"stages:
- stage: {FirstProdStage}
  jobs:
  - deployment: DeployWeb1
    environment: {FirstEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
  - deployment: DeployWeb2
    environment: {SecondEnvironment.Name}
    strategy:
      runOnce:
        deploy:
          steps:
          - script: echo Hello World!
";
}