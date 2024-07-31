using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public interface IPipelineRule : IRule
{
    Task<bool> EvaluateAsync(Domain.Compliancy.Pipeline pipeline);
}