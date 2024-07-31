using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation;

namespace Rabobank.Compliancy.Application.Requests;

public class ReconcileRequest : RequestBase
{
    /// <summary>
    /// The name of a specific rule.
    /// </summary>
    public string RuleName { get; init; }

    /// <summary>
    /// The item identifier represents the reference of a repository or a pipeline.
    /// </summary>
    public string ItemId { get; init; }

    private readonly ReconcileRequestValidator _validator = new();

    /// <inheritdoc/>
    public override ValidationResult Validate()
    {
        return _validator.Validate(this);
    }
}