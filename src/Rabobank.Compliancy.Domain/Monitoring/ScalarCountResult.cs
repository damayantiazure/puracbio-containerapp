#nullable enable

using System.ComponentModel.DataAnnotations;

namespace Rabobank.Compliancy.Domain.Monitoring;
public class ScalarCountResult
{
    [Display(Name = "Count")]
    public long Count { get; set; }
}
