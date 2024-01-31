#nullable enable

using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Interfaces.OpenPermissions;
public interface IOpenPipelinePermissionsProcess<TPipeline> : IOpenProtectedResourcePermissionsProcess<OpenPipelinePermissionsRequest<TPipeline>, TPipeline>
    where TPipeline : Pipeline
{
}
