#nullable enable

using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation.OpenPermissions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Requests.OpenPermissions;

public class OpenPipelinePermissionsRequest<TPipeline> : OpenPermissionsRequestBase<TPipeline>
    where TPipeline : Pipeline
{
    private readonly OpenPipelinePermissionsRequestValidator<TPipeline> _validator = new();

    public int PipelineId { get; set; }

    /// <inheritdoc/>
    public override ValidationResult Validate() => _validator.Validate(this);
}