using FluentValidation;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public class RescanPipelineRequestValidator : AbstractValidator<RescanPipelineRequest>
{
    public RescanPipelineRequestValidator()
    {
        RuleFor(request => request.Organization)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.PipelineId)
            .NotEmpty();
    }
}