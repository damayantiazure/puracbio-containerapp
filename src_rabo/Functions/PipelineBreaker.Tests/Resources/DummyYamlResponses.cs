namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Resources;

public static class DummyYamlResponses
{
    public static readonly string YamlPipelineWithStagesMultiStage = $@"            
            stages:
            - stage: test
              jobs:
              - job: JobName
                steps:
                - task: 
                  inputs:
            - stage: test2
              jobs:
              - job: JobName
                steps:
                - task: 
                  inputs:
                    ";

    public static readonly string YamlPipelineWithStagesSingleStage = $@"
            stages:
            - stage: test
              jobs:
              - job: JobName
                steps:
                - task: 
                  inputs:
                    ";

    public static readonly string StagelessYamlPipeline = $@"
            stages:
            - stage: __default
              jobs:
              - job: JobName
                steps:
                - task: 
                  inputs:
                    ";
}