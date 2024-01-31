using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation;

namespace Rabobank.Compliancy.Application.Requests;

public class DeleteDeviationRequest : RequestBase
{
    /// <summary>
    /// The getter and setter for the identifier of the configuration item.
    /// </summary>
    public string CiIdentifier { get; set; }

    /// <summary>
    /// The getter and setter for the name of the rule.
    /// </summary>
    public string RuleName { get; set; }

    /// <summary>
    /// The getter and setter for the pipeline or the repository identifier.
    /// </summary>
    public string ItemId { get; set; }

    /// <summary>
    /// The getter and setter for the linked project identifier.
    /// </summary>
    public Guid? ForeignProjectId { get; set; }

    private readonly DeleteDeviationRequestValidator _validator = new();

    /// <inheritdoc/>
    public override ValidationResult Validate() =>
        _validator.Validate(this);
}