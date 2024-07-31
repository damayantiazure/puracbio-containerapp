#nullable enable
using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation;

namespace Rabobank.Compliancy.Application.Requests;

public class ExclusionListRequest : RequestBase
{
    /// <summary>
    /// The getter and setter for the pipeline identifier.
    /// </summary>
    public int PipelineId { get; set; } = default!;

    /// <summary>
    /// The getter and setter for the pipeline type.
    /// </summary>
    public string PipelineType { get; set; } = default!;

    /// <summary>
    /// The getter and setter for the reason of the exclusion.
    /// </summary>
    public string? Reason { get; set; }

    private readonly ExclusionListRequestValidator _validator = new();

    /// <inheritdoc/>
    public override ValidationResult Validate() =>
        _validator.Validate(this);
}