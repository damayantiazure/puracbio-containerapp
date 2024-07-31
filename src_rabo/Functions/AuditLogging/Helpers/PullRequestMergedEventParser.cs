using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.Approvals.Utils;
using Rabobank.Compliancy.Functions.AuditLogging.Model;
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public class PullRequestMergedEventParser : IPullRequestMergedEventParser
{
    private const int OrganizationUrlSegment = 1;
    private const int ApprovedVote = 10;

    public PullRequestMergedEvent Parse(string json)
        => string.IsNullOrWhiteSpace(json) ? null : Parse(JObject.Parse(json));

    private static PullRequestMergedEvent Parse(JToken jObject)
    {
        if (jObject == null)
        {
            return null;
        }

        return new PullRequestMergedEvent
        {
            Organization = GetUrlSegment((string)jObject.SelectToken("resource._links.web.href"), OrganizationUrlSegment),
            ProjectId = (string)jObject.SelectToken("resource.repository.project.id"),
            ProjectName = (string)jObject.SelectToken("resource.repository.project.name"),
            PullRequestId = (string)jObject.SelectToken("resource.pullRequestId"),
            PullRequestUrl = (string)jObject.SelectToken("resource.url"),
            RepositoryId = (string)jObject.SelectToken("resource.repository.id"),
            RepositoryUrl = (string)jObject.SelectToken("resource.repository.url"),
            Status = (string)jObject.SelectToken("resource.status"),
            CreationDate = GetDate(jObject.SelectToken("resource.creationDate")),
            ClosedDate = GetDate(jObject.SelectToken("resource.closedDate")),
            Approvers = GetApprovers(jObject.SelectToken("resource.reviewers")),
            LastMergeCommitId = (string)jObject.SelectToken("resource.lastMergeCommit.commitId"),
            LastMergeSourceCommit = (string)jObject.SelectToken("resource.lastMergeSourceCommit.commitId"),
            LastMergeTargetCommit = (string)jObject.SelectToken("resource.lastMergeTargetCommit.commitId"),
            CreatedBy = (string)jObject.SelectToken("resource.createdBy.uniqueName"),
            ClosedBy = (string)jObject.SelectToken("resource.closedBy.uniqueName")
        };
    }

    private static IEnumerable<string> GetApprovers(JToken jToken)
    {
        var array = jToken as JArray;

        var result = new List<string>();

        foreach (var item in array)
        {
            var isParseSuccess = int.TryParse(item["vote"].ToString(), out var voteResult);

            if (isParseSuccess && voteResult == ApprovedVote && MailChecker.IsValidEmail((string)item["uniqueName"]))
            {
                result.Add((string)item["uniqueName"]);
            }
        }

        return result;
    }

    private static DateTime GetDate(JToken token) =>
        token == null ? DateTime.MinValue : (DateTime)token;

    private static string GetUrlSegment(string runUrl, int segment) =>
        new Uri(runUrl).Segments[segment].TrimEnd('/');
}