#nullable enable

using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;

namespace Rabobank.Compliancy.Core.InputValidation.Services;

public class ValidateInputService : IValidateInputService
{
    public void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization,
        [NotNull] string? projectId)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(organization))
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new ArgumentNullException(nameof(projectId));
        }
    }

    public void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization,
        [NotNull] string? projectId, [NotNull] string? itemId)
    {
        Validate(request, organization, projectId);

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentNullException(nameof(itemId));
        }
    }

    public void Validate([NotNull] string? organization, [NotNull] string? projectId,
        [NotNull] string? ciOrPipelineIdentifier, [NotNull] HttpRequestMessage? request)
    {
        Validate(request, organization, projectId);

        if (string.IsNullOrWhiteSpace(ciOrPipelineIdentifier))
        {
            throw new ArgumentNullException(nameof(ciOrPipelineIdentifier));
        }
    }

    public void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization,
        [NotNull] string? projectId,
        [NotNull] string? ruleName, [NotNull] string? itemId)
    {
        Validate(request, organization, projectId, itemId);

        if (string.IsNullOrWhiteSpace(ruleName))
        {
            throw new ArgumentNullException(nameof(ruleName));
        }
    }

    public void Validate([NotNull] string? organization, [NotNull] string? projectId,
        [NotNull] string? runId, [NotNull] string? stageId, [NotNull] HttpRequestMessage? request)
    {
        Validate(request, organization, projectId, runId);

        if (string.IsNullOrWhiteSpace(stageId))
        {
            throw new ArgumentNullException(nameof(stageId));
        }
    }

    public void Validate([NotNull] HttpRequestMessage? request, [NotNull] string? organization,
        [NotNull] string? projectId,
        [NotNull] string? ruleName, [NotNull] string? itemId, [NotNull] string? ciIdentifier)
    {
        Validate(request, organization, projectId, itemId);

        if (string.IsNullOrWhiteSpace(ruleName))
        {
            throw new ArgumentNullException(nameof(ruleName));
        }

        if (string.IsNullOrWhiteSpace(ciIdentifier))
        {
            throw new ArgumentNullException(nameof(ciIdentifier));
        }
    }

    public ActionResult ValidateInput(string? projectId, string? id, string? organizationUri, bool release)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A {nameof(projectId)} was not provided in the URL."));
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A {(release ? "releaseId" : "runId")} was not provided in the URL."));
        }

        if (!id.All(char.IsDigit))
        {
            return new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"The {(release ? "releaseId" : "runId")}: '{id}' provided in the URL is invalid. " +
                $"It should only consist of numbers."));
        }

        if (string.IsNullOrWhiteSpace(organizationUri))
        {
            return new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A 'PlanUrl' was not provided in the request header. " +
                $"PlanUrls can be provided by adding following to your request header:\n" +
                $"PlanUrl: $(system.CollectionUri)"));
        }

        var organization = organizationUri.GetAzdoOrganizationName();
        if (string.IsNullOrWhiteSpace(organization))
        {
            return new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"The PlanUrl: '{organizationUri}' provided in the request header is invalid. " +
                $"It should be an URL pointing towards the AzDO-organization where your project is hosted."));
        }

        return new OkObjectResult(organization);
    }

    public void ValidateItemType(string itemType, string[] validTypes)
    {
        if (!validTypes.Any(t => string.Equals(t, itemType, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new ArgumentOutOfRangeException(nameof(itemType));
        }
    }
}