using AutoFixture;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Core.Rules.Tests;

public class ServiceCollectionTests
{
    private readonly IFixture _fixture = new Fixture();
    [Fact]
    public void GlobalPermissions()
    {
        var service = new ServiceCollection()
            .AddDefaultRules()
            .AddSingleton(Substitute.For<IAzdoRestClient>())
            .AddSingleton(Create.MockedMemoryCache())
            .BuildServiceProvider();

        var rules = service.GetServices<IProjectRule>();
        rules.OfType<NobodyCanDeleteTheProject>().ShouldNotBeEmpty();
    }

    [Fact]
    public void RepositoryRules()
    {
        var service = new ServiceCollection()
            .AddDefaultRules()
            .AddSingleton(Substitute.For<IAzdoRestClient>())
            .AddSingleton(Create.MockedMemoryCache())
            .BuildServiceProvider();

        var rules = service.GetServices<IRepositoryRule>().ToList();
        rules
            .OfType<NobodyCanDeleteTheRepository>()
            .ShouldNotBeEmpty();
    }

    [Fact]
    public void BuildRules()
    {
        var service = new ServiceCollection()
            .AddDefaultRules()
            .AddSingleton(Substitute.For<IAzdoRestClient>())
            .AddSingleton(Create.MockedMemoryCache())
            .AddSingleton<IPipelineEvaluatorFactory, PipelineEvaluatorFactory>()
            .AddSingleton<IYamlHelper, YamlHelper>()
            .BuildServiceProvider();

        var rules = service.GetServices<IBuildPipelineRule>().ToList();
        rules
            .OfType<NobodyCanDeleteBuilds>()
            .ShouldNotBeEmpty();
    }

    [Fact]
    public void ReleaseRules()
    {
        var service = new ServiceCollection()
            .AddDefaultRules()
            .AddSingleton(Substitute.For<IAzdoRestClient>())
            .AddSingleton(Substitute.For<IPipelineRegistrationResolver>())
            .AddSingleton(Create.MockedMemoryCache())
            .AddSingleton(Substitute.For<RuleConfig>())
            .BuildServiceProvider();

        var rules = service.GetServices<IClassicReleasePipelineRule>().ToList();
        rules
            .OfType<NobodyCanDeleteReleases>()
            .ShouldNotBeEmpty();
    }

    [Fact()]
    public void AllRulesShouldBeInProvider()
    {
        var service = new ServiceCollection()
            .AddDefaultRules();

        var types = typeof(IRule).Assembly.GetTypes().Where(t => typeof(IRule).IsAssignableFrom(t) && !t.IsInterface).ToList();
        types.ShouldNotBeEmpty();

        types.ShouldAllBe(t => service.Select(r => r.ImplementationType).Contains(t));
    }

    [Fact]
    public void DefaultRuleProfile_WithAllDefinedAndImplementedRules_ShouldReturnTrue()
    {
        // Arrange
        var allRules = GetAllRules().Select(x => x.Name);

        // Act
        var actual = _fixture.Create<DefaultRuleProfile>().Rules;

        // Assert
        Compare(actual, allRules).ShouldBeTrue();
    }

    [Fact]
    public void MainFrameCobolRuleProfile_WithAllDefinedAndImplementedRules_ShouldReturnTrue()
    {
        // Arrange
        var allRules = GetAllRules().Select(x => x.Name);

        // Act
        var actual = _fixture.Create<MainFrameCobolRuleProfile>().Rules;

        // Assert
        Compare(actual, allRules).ShouldBeTrue();
    }

    [Fact]
    public void MainFrameCobolRuleProfile_WithNewRuleDefinedWithNoImplementedRules_ShouldReturnFalse()
    {
        // Arrange
        var newRuleName = _fixture.CreateMany<string>(1);
        var allRules = GetAllRules().Select(x => x.Name);

        // Act
        var actual = _fixture.Create<MainFrameCobolRuleProfile>().Rules.Concat(newRuleName);

        // Assert
        Compare(actual, allRules).ShouldBeFalse();
    }

    [Fact]
    public void DefaultRuleProfile_WithNewRuleDefinedWithNoImplementedRules_ShouldReturnFalse()
    {
        // Arrange
        var newRuleName = _fixture.CreateMany<string>(1);
        var allRules = GetAllRules().Select(x => x.Name);

        // Act
        var actual = _fixture.Create<DefaultRuleProfile>().Rules.Concat(newRuleName);

        // Assert
        Compare(actual, allRules).ShouldBeFalse();
    }

    private static ServiceProvider SetupServiceCollection()
    {
        var service = new ServiceCollection()
            .AddDefaultRules()
            .AddSingleton(Substitute.For<IAzdoRestClient>())
            .AddSingleton(Create.MockedMemoryCache())
            .AddSingleton(Substitute.For<IYamlHelper>())
            .AddSingleton(Substitute.For<IPipelineEvaluatorFactory>())
            .AddSingleton(Substitute.For<IPipelineRegistrationResolver>())
            .AddSingleton(Substitute.For<RuleConfig>())
            .AddSingleton(Substitute.For<IYamlEnvironmentHelper>())
            .BuildServiceProvider();

        return service;
    }

    private static bool Compare(IEnumerable<string> actual, IEnumerable<string> expected)
    {
        var result = true;
        foreach (var actualItem in actual)
        {
            if (!expected.Contains(actualItem))
            {
                return false;
            }
        }

        return result;
    }

    private static IEnumerable<IRule> GetAllRules()
    {
        var provider = SetupServiceCollection();
        var allRules = new List<IRule>();
        var buildPipelineRules = provider.GetServices<IBuildPipelineRule>();
        var repositoryRules = provider.GetServices<IRepositoryRule>();
        var classicReleasePipelineRules = provider.GetServices<IClassicReleasePipelineRule>();
        var yamlReleasePipelineRules = provider.GetServices<IYamlReleasePipelineRule>();
        var projectRules = provider.GetServices<IProjectRule>();

        allRules.AddRange(buildPipelineRules);
        allRules.AddRange(repositoryRules);
        allRules.AddRange(classicReleasePipelineRules);
        allRules.AddRange(yamlReleasePipelineRules);
        allRules.AddRange(projectRules);

        return allRules;
    }
}