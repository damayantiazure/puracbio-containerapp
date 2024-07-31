using FluentValidation.Results;
using Rabobank.Compliancy.Application.Requests.RequestValidation;

namespace Rabobank.Compliancy.Application.Requests;

public abstract class RequestBase : IValidatable
{
    /// <summary>
    /// The name of the organization to be used.
    /// </summary>
    public string Organization { get; init; }

    /// <summary>
    /// The project identifier to be used.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <inheritdoc/>
    public abstract ValidationResult Validate();
}