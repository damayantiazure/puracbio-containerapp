using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class Hooks
{
    private static readonly IDictionary<string, object> CommonApiVersion = new Dictionary<string, object>
    {
        {"api-version", "6.0"}
    };

    // Retrieving all hooks for Raboweb takes at least 1.5 minute.
    // It is essential to complete call, otherwise new webhooks for audit logging are possibly not created
    public static IEnumerableRequest<Hook> Subscriptions() =>
        new AzdoRequest<Hook>($"_apis/hooks/subscriptions", CommonApiVersion, 600).AsEnumerable();

    public static class Add
    {
        public static Body BuildCompleted(string accountName, string accountKey,
            string queueName, string projectId)
        {
            var publisherInputs = new BuildCompletePublisherInputs
            {
                DefinitionName = "",
                ProjectId = projectId,
                BuildStatus = "",
            };
            return NewBody(accountName, accountKey, queueName, "build.complete", "tfs", publisherInputs, "1.0");
        }

        public static Body GitPullRequestCreated(string accountName, string accountKey, string queueName, string projectId)
        {
            var publisherInputs = new PullRequestCreatedPublisherInputs
            {
                ProjectId = projectId,
                Repository = "",
                Branch = "",
                PullrequestCreatedBy = "",
                PullrequestReviewersContains = ""
            };
            return NewBody(accountName, accountKey, queueName, "git.pullrequest.created", "tfs", publisherInputs, "1.0");
        }

        public static Body GitPullRequestMerged(string accountName, string accountKey, string queueName, string projectId)
        {
            var publisherInputs = new PullRequestMergedPublisherInputs
            {
                ProjectId = projectId,
                Repository = "",
                Branch = "",
                PullrequestReviewersContains = "",
                MergeResult = "Succeeded"
            };
            return NewBody(accountName, accountKey, queueName, "git.pullrequest.merged", "tfs", publisherInputs, "1.0");
        }

        public static Body GitPushed(string accountName, string accountKey, string queueName, string projectId)
        {
            var publisherInputs = new GitPushPublisherInputs
            {
                ProjectId = projectId,
                Repository = "",
                Branch = "",
                PushedBy = "",
            };
            return NewBody(accountName, accountKey, queueName, "git.push", "tfs", publisherInputs, "1.0");
        }

        public static Body ReleaseDeploymentCompleted(
            string accountName, string accountKey, string queueName, string projectId, string pipelineId)
        {
            var publisherInputs = new ReleaseDeploymentCreatedPublisherInputs
            {
                ProjectId = projectId,
                ReleaseDefinitionId = pipelineId,
                ReleaseEnvironmentId = "",
                ReleaseEnvironmentStatus = "",
            };
            return NewBody(accountName, accountKey, queueName, "ms.vss-release.deployment-completed-event",
                "rm", publisherInputs, "3.0-preview.1", "minimal");
        }

        public static Body RunStageCompleted(
            string accountName, string accountKey, string queueName, string projectId, string pipelineId)
        {
            var publisherInputs = new RunStageStateChangedPublisherInputs
            {
                ProjectId = projectId,
                PipelineId = pipelineId,
                StageNameId = "",
                StageStateId = "Completed",
            };
            return NewBody(accountName, accountKey, queueName, "ms.vss-pipelines.stage-state-changed-event",
                "pipelines", publisherInputs, "5.1-preview.1");
        }

        public class ConsumerInputs
        {
            public string AccountName { get; set; }
            public string AccountKey { get; set; }
            public string QueueName { get; set; }
            public string VisiTimeout { get; set; }
            public string Ttl { get; set; }
            public string ResourceDetailsToSend { get; set; }
        }

        public class PublisherInputs
        {
            protected PublisherInputs()
            {
            }
            public string ProjectId { get; set; }
        }

        public class Body
        {
            public string ConsumerActionId { get; set; }
            public string ConsumerId { get; set; }
            public ConsumerInputs ConsumerInputs { get; set; }
            public string EventType { get; set; }
            public string PublisherId { get; set; }
            public PublisherInputs PublisherInputs { get; set; }
            public string ResourceVersion { get; set; }
            public int Scope { get; set; }
        }

        private class ReleaseDeploymentCreatedPublisherInputs : PublisherInputs
        {
            public string ReleaseDefinitionId { get; set; }
            public string ReleaseEnvironmentId { get; set; }
            public string ReleaseEnvironmentStatus { get; set; }
        }

        private class RunStageStateChangedPublisherInputs : PublisherInputs
        {
            public string PipelineId { get; set; }
            public string StageNameId { get; set; }
            public string StageStateId { get; set; }
        }

        private class PullRequestCreatedPublisherInputs : PublisherInputs
        {
            public string Repository { get; set; }
            public string Branch { get; set; }
            public string PullrequestCreatedBy { get; set; }
            public string PullrequestReviewersContains { get; set; }
        }

        private class PullRequestMergedPublisherInputs : PublisherInputs
        {
            public string Repository { get; set; }
            public string Branch { get; set; }
            public string PullrequestReviewersContains { get; set; }
            public string MergeResult { get; set; }
        }

        private class GitPushPublisherInputs : PublisherInputs
        {
            public string Repository { get; set; }
            public string Branch { get; set; }
            public string PushedBy { get; set; }
        }

        private class BuildCompletePublisherInputs : PublisherInputs
        {
            public string DefinitionName { get; set; }
            public string BuildStatus { get; set; }
        }

        [SuppressMessage("Sonar Code Smell",
            "S107: Constructor has 8 parameters, which is greater than the 7 authorized.",
            Justification = "We will allow it because it is hard to reduce the number of arguments. Also, the implementation is very simple so the number of parameters should not be a problem in terms of mental load.")]
        private static Body NewBody(string accountName, string accountKey, string queueName, string eventType,
            string publisherId, PublisherInputs publisherInputs, string resourceVersion, string resourceDetailsToSend = "all") =>
            new()
            {
                ConsumerActionId = "enqueue",
                ConsumerId = "azureStorageQueue",
                ConsumerInputs = new ConsumerInputs
                {
                    AccountName = accountName,
                    AccountKey = accountKey,
                    QueueName = queueName,
                    VisiTimeout = "0",
                    Ttl = "604800",
                    ResourceDetailsToSend = resourceDetailsToSend
                },
                EventType = eventType,
                PublisherId = publisherId,
                PublisherInputs = publisherInputs,
                ResourceVersion = resourceVersion,
                Scope = 1,
            };
    }

    public static IAzdoRequest<Hook> Subscription(string id) =>
        new AzdoRequest<Hook>($"_apis/hooks/subscriptions/{id}", CommonApiVersion);
    public static IAzdoRequest<Add.Body, Hook> AddHookSubscription() =>
        new AzdoRequest<Add.Body, Hook>($"_apis/hooks/subscriptions", CommonApiVersion);
    public static IAzdoRequest<Add.Body, Hook> AddReleaseManagementSubscription() =>
        new VsrmRequest<Add.Body, Hook>($"_apis/hooks/subscriptions", CommonApiVersion);

    public static IEnumerableRequest<Notification> HookNotifications(
        string hookId, NotificationResult result) =>
        new AzdoRequest<Notification>($"_apis/hooks/subscriptions/{hookId}/notifications/",
            new Dictionary<string, object>
            {
                {"result", result},
                {"api-version", "6.0"}
            }).AsEnumerable();

    public static IEnumerableRequest<Notification> HookNotifications(string hookId) =>
        new AzdoRequest<Notification>($"_apis/hooks/subscriptions/{hookId}/notifications/",
            CommonApiVersion).AsEnumerable();

    public static IAzdoRequest<Notification> HookNotification(
        string hookId, int notificationId) =>
        new AzdoRequest<Notification>($"_apis/hooks/subscriptions/{hookId}/notifications/{notificationId}",
            CommonApiVersion);
}