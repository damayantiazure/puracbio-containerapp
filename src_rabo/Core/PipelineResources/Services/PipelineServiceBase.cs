using Flurl.Http;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.PipelineResources.Services;

public abstract class PipelineServiceBase
{
    private readonly IAzdoRestClient _azdoClient;

    protected PipelineServiceBase(IAzdoRestClient azdoClient)
    {
        _azdoClient = azdoClient;
    }

    protected async Task<BuildDefinition> GetReferencedMainframeCobolPipeline(Dictionary<string, string> inputs)
    {
        var refOrganizationName = inputs.GetValueOrDefault(TaskContants.MainframeCobolConstants.OrganizationName);
        var refProjectId = inputs.GetValueOrDefault(TaskContants.MainframeCobolConstants.ProjectId);
        var refPipelineId = inputs.GetValueOrDefault(TaskContants.MainframeCobolConstants.PipelineId);

        if (!Guid.TryParse(refProjectId, out _))
        {
            return null;
        }

        try
        {
            var referencedBuildDefinition = await _azdoClient
                .GetAsync(Builds.BuildDefinition(refProjectId, refPipelineId), refOrganizationName);

            return referencedBuildDefinition;
        }
        catch (FlurlHttpException ex) when (ex.Call?.HttpStatus == HttpStatusCode.BadRequest)
        {
            return null;
        }
    }

    protected static RuleProfile GetRuleProfileForYamlRelease(IEnumerable<PipelineRegistration> registrations)
    {
        RuleProfile profile = new DefaultRuleProfile();
        if (registrations == null || !registrations.Any())
        {
            return profile;
        }

        var registration = registrations.FirstOrDefault();

        if (registration != null)
        {
            profile = registration.GetRuleProfile();
        }
        return profile;
    }
}