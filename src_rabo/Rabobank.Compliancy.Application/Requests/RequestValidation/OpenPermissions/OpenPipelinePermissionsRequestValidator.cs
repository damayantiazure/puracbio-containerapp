using FluentValidation;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation.OpenPermissions;

public class OpenPipelinePermissionsRequestValidator<TPipeline> : AbstractRequestBaseValidator<OpenPipelinePermissionsRequest<TPipeline>>
    where TPipeline : Pipeline
{
    public OpenPipelinePermissionsRequestValidator()
    {
        RuleFor(x => x.PipelineId).GreaterThan(0).WithMessage(x => $"'{nameof(x.PipelineId)}' must be greater than zero.");
    }
}