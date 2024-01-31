using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Helpers;

public class PipelineEvaluatorFactoryTests
{
    [Theory]
    [InlineData(1, typeof(ClassicPipelineEvaluator))]
    [InlineData(2, typeof(YamlPipelineEvaluator))]
    public void CanCreatePipelineEvaluator(int buildProcessType, Type evaluatorType)
    {
        // arrange
        var factory = new PipelineEvaluatorFactory(null, null, null);
        var buildDefinition = new BuildDefinition { Process = new BuildProcess { Type = buildProcessType } };

        // act 
        var evaluator = factory.Create(buildDefinition);

        // assert
        evaluator.Should().BeOfType(evaluatorType);
    }
}