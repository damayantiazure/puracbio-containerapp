using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.PipelineResources.Services;

public interface IRepositoryService
{
    public Task<Uri> GetUrlAsync(string organization, Project pipelineProject, Repository repository);
    public Task<string> GetProjectIdByNameAsync(string organization, Project pipelineProject, Repository repository);
}