using AutoFixture;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[SuppressMessage("Code Smell", "xUnit1033:Test classes decorated with 'Xunit.IClassFixture' or 'Xunit.ICollectionFixture' should add a constructor argument of type TFixture.", 
    Justification = "HookFixtureAsync does not use instance data and can be static.")]
[Trait("category", "integration")]
public class HooksTests : IClassFixture<HooksTests.HookFixture>
{
    [Fact]
    public async Task QuerySubscriptions()
    {
        var fixture = await HookFixture.HookFixtureAsync();
        var subscriptions = await fixture.Client.GetAsync(Hooks.Subscriptions());
        subscriptions.ShouldAllBe(_ => !string.IsNullOrEmpty(_.Id));
    }

    [Fact]
    public async Task BuildCompleted()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var body = Hooks.Add.BuildCompleted(
            fixture.AccountName,
            fixture.AccountKey,
            fixture.QueueName,
            fixture.ProjectId
        );

        var hook = await fixture.Client.PostAsync(Hooks.AddHookSubscription(), body);
        hook.Id.ShouldNotBeNullOrEmpty();

        await fixture.Client.DeleteAsync(Hooks.Subscription(hook.Id));
    }

    [Fact]
    public async Task GitPushed()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var body = Hooks.Add.GitPushed(
            fixture.AccountName,
            fixture.AccountKey,
            fixture.QueueName,
            fixture.ProjectId
        );

        var hook = await fixture.Client.PostAsync(Hooks.AddHookSubscription(), body);
        hook.Id.ShouldNotBeNullOrEmpty();

        await fixture.Client.DeleteAsync(Hooks.Subscription(hook.Id));
    }

    [Fact]
    public async Task GitPullRequestCreated()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var body = Hooks.Add.GitPullRequestCreated(
            fixture.AccountName,
            fixture.AccountKey,
            fixture.QueueName,
            fixture.ProjectId
        );

        var hook = await fixture.Client.PostAsync(Hooks.AddHookSubscription(), body);
        hook.Id.ShouldNotBeNullOrEmpty();
        hook.PublisherInputs.PullrequestCreatedBy.ShouldNotBeNull();

        await fixture.Client.DeleteAsync(Hooks.Subscription(hook.Id));
    }

    [Fact]
    public async Task GitPullRequestMerged()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var body = Hooks.Add.GitPullRequestMerged(
            fixture.AccountName,
            fixture.AccountKey,
            fixture.QueueName,
            fixture.ProjectId
        );

        var hook = await fixture.Client.PostAsync(Hooks.AddHookSubscription(), body);
        hook.Id.ShouldNotBeNullOrEmpty();

        await fixture.Client.DeleteAsync(Hooks.Subscription(hook.Id));
    }

    [Fact]
    public async Task ReleaseDeploymentCompleted()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var body = Hooks.Add.ReleaseDeploymentCompleted(
            fixture.AccountName,
            fixture.AccountKey,
            fixture.QueueName,
            fixture.ProjectId,
            fixture.ReleaseDefinitionId
        );

        var hook = await fixture.Client.PostAsync(Hooks.AddReleaseManagementSubscription(), body);
        hook.Id.ShouldNotBeNullOrEmpty();

        await fixture.Client.DeleteAsync(Hooks.Subscription(hook.Id));
    }

    [Fact]
    public async Task RunStageStateChanged()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var body = Hooks.Add.RunStageCompleted(
            fixture.AccountName,
            fixture.AccountKey,
            fixture.QueueName,
            fixture.ProjectId,
            fixture.PipelineId
        );

        var hook = await fixture.Client.PostAsync(Hooks.AddHookSubscription(), body);
        hook.Id.ShouldNotBeNullOrEmpty();

        await fixture.Client.DeleteAsync(Hooks.Subscription(hook.Id));
    }

    [SuppressMessage("Code Smell", "xUnit1004:Test methods should not be skipped.", Justification = "Test cannot run.")]
    [Fact(Skip = "Notifications on hooks are cleared after a while")]
    public async Task GetHookHistory()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var notifications = (await fixture.Client.GetAsync(Hooks.HookNotifications(
            "9642da48-20e7-4905-a53d-6fa8fee0ec18", NotificationResult.failed))).ToList();
        notifications.ShouldNotBeEmpty();

        var notification = notifications[0];
        notification.Result.ShouldBe(NotificationResult.failed);
        notification.SubscriptionId.ShouldNotBeNull();
    }

    [SuppressMessage("Code Smell", "xUnit1004:Test methods should not be skipped.", Justification = "Test cannot run.")]
    [Fact(Skip = "Notifications on hooks are cleared after a while")]
    public async Task GetHookHistoryWithDetails()
    {
        var fixture = await HookFixture.HookFixtureAsync();

        var notifications = (await fixture.Client.GetAsync(Hooks.HookNotifications(
            "9642da48-20e7-4905-a53d-6fa8fee0ec18", NotificationResult.failed))).ToList();
        notifications.ShouldNotBeEmpty();

        var notification = await fixture.Client.GetAsync(Hooks.HookNotification(
            "9642da48-20e7-4905-a53d-6fa8fee0ec18", notifications[0].Id));

        notification.Result.ShouldBe(NotificationResult.failed);
        notification.SubscriptionId.ShouldNotBeNull();
        notification.Details.ShouldNotBeNull();
        notification.Details.ErrorDetail.ShouldNotBeNull();
        notification.Details.ErrorMessage.ShouldNotBeNull();
        notification.Details.Event.ShouldNotBeNull();
        notification.Details.Event.Id.ShouldNotBeNull();
        notification.Details.Event.Resource.ShouldNotBeNull();
        notification.Details.PublisherInputs.ShouldNotBeNull();
        notification.Details.PublisherInputs.ProjectId.ShouldNotBeNull();
        notification.Details.PublisherInputs.ReleaseDefinitionId.ShouldNotBeNull();
    }

    public class HookFixture : IDisposable
    {
        public string AccountKey { get; } = "01234156789123456784564560123415678912345678456456123456789123456";
        public string AccountName { get; set; }
        public string QueueName { get; } = "queuename";
        public string ProjectId { get; set; }
        public string PipelineId { get; set; }
        public string ReleaseDefinitionId { get; set; }

        public IAzdoRestClient Client { get; set; }

        public static async Task<HookFixture> HookFixtureAsync()
        {
            var config = new TestConfig();

            var result = new HookFixture
            {
                Client = new AzdoRestClient(config.Organization, config.Token),
                ProjectId = config.ProjectId
            };
            result.PipelineId = (await result.Client.GetAsync(Builds.BuildDefinitions(result.ProjectId))).First().Id;
            result.ReleaseDefinitionId = (await result.Client.GetAsync(ReleaseManagement.Definitions(result.ProjectId))).First().Id;


            var fixture = new Fixture();
            result.AccountName = fixture.Create("integration-test-hook");
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            // Make sure all hooks from this test run are properly deleted.
            (await Client
                    .GetAsync(Hooks.Subscriptions()))
                .ShouldNotContain(x => x.ConsumerInputs.AccountName == AccountName);
        }
    }
}