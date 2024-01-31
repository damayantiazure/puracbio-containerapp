using System.Collections.Generic;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response.Interfaces;

public interface IRegisterableDefinition
{
    string Id { get; set; }
    IEnumerable<PipelineRegistration> PipelineRegistrations { get; set; }
    IEnumerable<string> GetStageIds();
    IEnumerable<string> GetRegisteredStageIds();
    IEnumerable<Stage> GetStages();
}