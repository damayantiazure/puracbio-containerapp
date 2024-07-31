#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class ExceptionSummaryDto
{
    [JsonProperty("exceptionType")]
    public string? ExceptionType { get; set; }

    [JsonProperty("exceptionMessage")]
    public string? ExceptionMessage { get; set; }

    [JsonProperty("innerExceptionType")]
    public string? InnerExceptionType { get; set; }

    [JsonProperty("innerExceptionMessage")]
    public string? InnerExceptionMessage { get; set; }
}