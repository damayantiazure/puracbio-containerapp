#nullable enable
using FluentValidation;
using Rabobank.Compliancy.Application.Requests.RequestValidation.Extensions;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public class ExclusionListRequestValidator : AbstractRequestBaseValidator<ExclusionListRequest>
{
    public ExclusionListRequestValidator()
    {
        RuleFor(request => request.PipelineId)
            .NotNull().NotEmpty();

        RuleFor(request => request.PipelineType)
            .NotNull().NotEmpty().IsValidPipelineType();

        RuleFor(request => request.Reason)
          .NotNull().NotEmpty();
    }
}