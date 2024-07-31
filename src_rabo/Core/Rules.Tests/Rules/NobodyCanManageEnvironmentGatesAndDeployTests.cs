using AutoFixture;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Requests.YamlPipeline;
using Bits = Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class NobodyCanManageEnvironmentGatesAndDeployTests
{
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();
    private readonly Fixture _fixture = new Fixture { RepeatCount = 1 };

    [Theory]
    // Compliant setup: permissions are blocked for environment admins
    [InlineData("Production", "Administrator", PermissionLevelId.Deny, true)]
    [InlineData("Production", "Administrator", PermissionLevelId.DenyInherited, true)]

    // Incompliant setup: permissions are not blocked for environment admins
    [InlineData("Production", "Administrator", PermissionLevelId.Allow, false)]
    [InlineData("Production", "Administrator", PermissionLevelId.AllowInherited, false)]
    [InlineData("Production", "Administrator", PermissionLevelId.NotSet, false)]

    // Compliant setup: there is no environment admin
    [InlineData("Production", "Reader", PermissionLevelId.Allow, true)]
    [InlineData("Production.lsrv9891", "Reader", PermissionLevelId.Allow, true)]

    // Incompliant setup: environment not found
    [InlineData("Unknown", "Reader", PermissionLevelId.Allow, false)]
    public async Task NobodyCanManageEnvironmentGatesAndDeploy(string environment, string role, int permissionId,
        bool isCompliant)
    {
        //Arrange
        _fixture.Customize<BuildDefinition>(ctx => ctx
            .Without(b => b.Yaml)
            .Without(b => b.YamlUsedInRun));

        var buildPipeline = _fixture.Create<BuildDefinition>();
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var environments = _fixture.Build<EnvironmentYaml>()
            .With(e => e.Name, "Production")
            .CreateMany();
        var securityGroups = _fixture.CreateMany<EnvironmentSecurityGroup>();
        securityGroups.First().Role.Name = role;
        securityGroups.First().Identity.DisplayName = "adminGroup";

        var client = Substitute.For<IAzdoRestClient>();
        var yamlResponse = new YamlPipelineResponse
        {
            FinalYaml = $"stages:\r\n- stage: Production\r\n  jobs:\r\n  - job: Prod\r\n    environment:\r\n      name: {environment}"
        };
        client
            .PostAsync(Arg.Any<IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse>>(),
                Arg.Any<YamlPipelineRequest>(), organization, true)
            .Returns(yamlResponse);
        client
            .GetAsync(Arg.Any<IEnumerableRequest<EnvironmentYaml>>(), organization)
            .Returns(environments);
        client
            .GetAsync(Arg.Any<IEnumerableRequest<EnvironmentSecurityGroup>>(), organization)
            .Returns(securityGroups);

        var resolver = Substitute.For<IPipelineRegistrationResolver>();
        resolver.ResolveProductionStagesAsync(organization, project.Id, buildPipeline.Id)
            .Returns(new[] { "Production" });
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(client, resolver);

        LookupPermissionData(client, permissionId);

        //Act
        //Act
        var rule = new NobodyCanManageEnvironmentGatesAndDeploy(client, _cache, yamlEnvironmentHelper);
        var result = await rule.EvaluateAsync(organization, project.Id, buildPipeline);

        //Assert
        result.ShouldBe(isCompliant);
    }

    private static void LookupPermissionData(IAzdoRestClient client, int permissionId)
    {
        client.GetAsync(Arg.Any<IAzdoRequest<ApplicationGroups>>(), Arg.Any<string>())
            .Returns(new ApplicationGroups()
            {
                Identities = new List<ApplicationGroup>
                {
                    new ApplicationGroup() { FriendlyDisplayName = "adminGroup" },
                    new ApplicationGroup() { FriendlyDisplayName = "anotherGroup" },
                    new ApplicationGroup() { FriendlyDisplayName = "groupC" }
                }
            });

        client.GetAsync(Arg.Any<IAzdoRequest<PermissionsSet>>(), Arg.Any<string>())
            .Returns(new PermissionsSet
            {
                Permissions = new[]
                {
                    new Permission
                    {
                        DisplayName = "Queue builds",
                        PermissionBit = Bits.BuildDefinitionBits.QueueBuilds,
                        NamespaceId = SecurityNamespaceIds.Build,
                        PermissionId = permissionId
                    },
                    new Permission
                    {
                        DisplayName = "Contribute",
                        PermissionBit = Bits.RepositoryBits.Contribute,
                        NamespaceId = SecurityNamespaceIds.GitRepositories,
                        PermissionId = permissionId
                    },
                    new Permission
                    {
                        DisplayName = "Force push",
                        PermissionBit = Bits.RepositoryBits.ForcePush,
                        NamespaceId = SecurityNamespaceIds.GitRepositories,
                        PermissionId = permissionId
                    }
                }
            });
    }
}