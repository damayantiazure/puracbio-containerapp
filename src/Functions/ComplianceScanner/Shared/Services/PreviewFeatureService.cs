using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.StorageClient;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class PreviewFeatureService : IPreviewFeatureService
{
    private static readonly string Table = "PreviewFeatures";
    private readonly IStorageRepository _storageRepository;

    public PreviewFeatureService(IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
        _storageRepository.CreateTable(Table);
    }

    public async Task<bool> PreviewFeatureEnabledAsync(string featureName, string projectId)
    {
        var previewFeature = await GetEntityAsync(featureName, projectId);
            
        if (previewFeature != null)
        {
            return true;
        }

        return false;
    }

    private async Task<PreviewFeature> GetEntityAsync(string featureName, string projectId)
    {
        var result = await _storageRepository.GetEntityAsync<PreviewFeature>(featureName, projectId);
        return result?.Result as PreviewFeature;
    }
}