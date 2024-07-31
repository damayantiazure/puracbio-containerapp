using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IPreviewFeatureService
{
    Task<bool> PreviewFeatureEnabledAsync(string featureName, string projectId);
}