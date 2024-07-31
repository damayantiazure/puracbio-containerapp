using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb;

public class CmdbClient : ICmdbClient
{
    public const string AzdoCompliancyCiName = "AZDO-COMPLIANCY";
    private readonly HttpClient _httpClient;
    public const string ChangeModel = "TAS0001";

    public CmdbClient(IHttpClientFactory httpClientFactory) =>
        _httpClient = httpClientFactory.CreateClient(nameof(CmdbClient));

    public async Task<ConfigurationItem?> GetCiAsync(string ciIdentifier)
    {
        var httpContent = new RetrieveCiByKeyRequest
        {
            Body = new RetrieveCiByKeyRequestBody
            {
                Key = new[] { ciIdentifier },
                RequestType = RequestType.CI,
                Source = AzdoCompliancyCiName
            }
        }.ToHttpContent();

        var httpResponseMessage = await _httpClient.PostAsync("retrieveCiInfoByKey", httpContent);
        httpResponseMessage.EnsureSuccessStatusCode();

        var responseObj = await httpResponseMessage.ToResponseObjectAsync<RetrieveCiByKeyResponse>();
        return responseObj?.CiInfo?.ConfigurationItem?.SingleOrDefault();
    }

    public async Task<AssignmentGroup?> GetAssignmentGroupAsync(string assignmentGroupName)
    {
        var httpContent = new RetrieveGroupByKeyRequest
        {
            Body = new RetrieveGroupByKeyRequestBody
            {
                Key = new[] { assignmentGroupName },
                Type = RequestType.Group,
                Source = AzdoCompliancyCiName
            }
        }.ToHttpContent();

        var httpResponseMessage = await _httpClient.PostAsync("retrieveGroupInfoByKey", httpContent);
        httpResponseMessage.EnsureSuccessStatusCode();

        var responseObj = await httpResponseMessage.ToResponseObjectAsync<RetrieveGroupByKeyResponse>();
        return responseObj?.GroupInfo?.AssignmentGroup?.SingleOrDefault();
    }

    public Task<IEnumerable<DeploymentInformation>?> GetDeploymentMethodAsync(string? ciName)
    {
        if (ciName == null)
        {
            throw new ArgumentNullException(nameof(ciName));
        }

        return GetDeploymentMethodImplAsync(ciName);
    }

    public Task<ManageDeploymentInformationResponse?> InsertDeploymentMethodAsync(DeploymentMethod? newMethod)
    {
        if (newMethod == null)
        {
            throw new ArgumentNullException(nameof(newMethod));
        }

        return InsertDeploymentMethodImplAsync(newMethod);
    }

    public Task<ManageDeploymentInformationResponse?> UpdateDeploymentMethodAsync(
        ConfigurationItem? configurationItem,
        SupplementaryInformation? currentMethod,
        DeploymentMethod? newMethod)
    {
        if (configurationItem == null)
        {
            throw new ArgumentNullException(nameof(configurationItem));
        }

        if (currentMethod == null)
        {
            throw new ArgumentNullException(nameof(currentMethod));
        }

        if (newMethod == null)
        {
            throw new ArgumentNullException(nameof(newMethod));
        }

        return UpdateDeploymentMethodImplAsync(configurationItem, currentMethod, newMethod);
    }

    public async Task<ManageDeploymentInformationResponse?> DeleteDeploymentMethodAsync(
        ConfigurationItem configurationItem,
        SupplementaryInformation methodToDelete)
    {
        var httpContent = new ManageDeploymentInformationRequest
        {
            Body = new ManageDeploymentInformationBody
            {
                Key = configurationItem.CiName,
                Type = UpdateType.Update,
                Source = AzdoCompliancyCiName,
                DeploymentInformations = new[]
                {
                    new DeploymentInformation
                    {
                        Method = DeploymentInformation.AzureDevOpsMethod,
                        Information = methodToDelete.ToString()
                    }
                },
                Update = "remove"
            }
        }.ToHttpContent();

        HttpResponseMessage? httpResponseMessage = null;

        try
        {
            httpResponseMessage = await _httpClient.PostAsync("managedeploymentinformation", httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.ToResponseObjectAsync<ManageDeploymentInformationResponse>();
        }
        catch (Exception ex)
        {
            throw new CmdbClientException(
                $"Error occurred in {nameof(DeleteDeploymentMethodAsync)}. " +
                $"endPoint='{httpResponseMessage?.RequestMessage?.RequestUri}'" +
                $"update='remove'", ex);
        }
    }

    public async Task<IEnumerable<CiContentItem>> GetAzDoCIsLegacyAsync()
    {
        const string applicationCiType = "Application";
        const string subApplicationCiType = "SubApplication";

        const int batchSize = 500;
        var allCis = new List<CiContentItem>();

        IEnumerable<CiContentItem>? ciList;

        var start = 1;
        do
        {
            // Get application CI types
            ciList = await GetAzDoCIsBatchLegacyAsync(applicationCiType, start, batchSize);
            start += batchSize;
            allCis = allCis.Concat(ciList).ToList();
        } while (ciList.Count() == batchSize);

        start = 1;
        do
        {
            // Get subapplication CI types
            ciList = await GetAzDoCIsBatchLegacyAsync(subApplicationCiType, start, batchSize);
            start += batchSize;
            allCis = allCis.Concat(ciList).ToList();
        } while (ciList.Count() == batchSize);

        return allCis.Where(d => d.IsProduction);
    }

    public async Task<IEnumerable<CiContentItem>> GetAzDoCIsAsync()
    {
        // This batchsize is recommended by the ITSM/SM9 team
        const int batchSize = 100;
        var start = 1;
        var remainingItems = 0;
        var allCis = new List<CiContentItem>();

        IEnumerable<CiContentItem> ciList;

        do
        {
            // Get application and subapplication CI types
            var response = await GetAzDoCIsBatchAsync(start, batchSize);
            var responseInformationList = response?.RetrieveCiByQuery?.Information ?? Array.Empty<RetrieveCiByQueryResponseInformation>();
            remainingItems = response?.RetrieveCiByQuery?.More ?? 0;
            ciList = responseInformationList?.ToCiContentItems() ?? Enumerable.Empty<CiContentItem>();
            start += batchSize;
            allCis = allCis.Concat(ciList).ToList();
        } while (remainingItems > 0);

        return allCis.Where(d => d.IsProduction);
    }

    private async Task<IEnumerable<DeploymentInformation>?> GetDeploymentMethodImplAsync(string ciName)
    {
        var httpContent = new ManageDeploymentInformationRequest
        {
            Body = new ManageDeploymentInformationBody
            {
                Key = ciName,
                Type = UpdateType.Retrieve,
                Source = AzdoCompliancyCiName
            }
        }.ToHttpContent();

        HttpResponseMessage? httpResponseMessage = null;

        try
        {
            httpResponseMessage = await _httpClient.PostAsync("managedeploymentinformation", httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();

            var responseObj = await httpResponseMessage.ToResponseObjectAsync<ManageDeploymentInformationResponse>();
            return responseObj?.ManageDeploymentInformation?.DeploymentInformations?.Select(deploymentInformation =>
                new DeploymentInformation
                {
                    Information = deploymentInformation.Information,
                    Method = deploymentInformation.Method
                });
        }
        catch (Exception ex)
        {
            throw new CmdbClientException(
                $"Error occurred in GetDeploymentMethodAsync. " +
                $"endPoint='{httpResponseMessage?.RequestMessage?.RequestUri}'" +
                $"retrieve='{ciName}'", ex);
        }
    }

    private async Task<ManageDeploymentInformationResponse?> InsertDeploymentMethodImplAsync(DeploymentMethod newMethod)
    {
        var httpContent = new ManageDeploymentInformationRequest
        {
            Body = new ManageDeploymentInformationBody
            {
                Key = newMethod.CiName,
                Type = UpdateType.Update,
                Source = AzdoCompliancyCiName,
                DeploymentInformations = new[]
                {
                    new DeploymentInformation
                    {
                        Method = DeploymentInformation.AzureDevOpsMethod,
                        Information = newMethod.ToString()
                    }
                }
            }
        }.ToHttpContent();

        HttpResponseMessage? httpResponseMessage = null;

        try
        {
            httpResponseMessage = await _httpClient.PostAsync("managedeploymentinformation", httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.ToResponseObjectAsync<ManageDeploymentInformationResponse>();
        }
        catch (Exception ex)
        {
            throw new CmdbClientException(
                $"Error occurred in {nameof(InsertDeploymentMethodAsync)}. " +
                $"endPoint='{httpResponseMessage?.RequestMessage?.RequestUri}'" +
                $"update='{newMethod}'", ex);
        }
    }

    private async Task<ManageDeploymentInformationResponse?> UpdateDeploymentMethodImplAsync(
        ConfigurationItem configurationItem,
        SupplementaryInformation currentMethod,
        DeploymentMethod newMethod)
    {
        var httpContent = new ManageDeploymentInformationRequest
        {
            Body = new ManageDeploymentInformationBody
            {
                Key = configurationItem.CiName,
                Type = UpdateType.Update,
                Source = AzdoCompliancyCiName,
                DeploymentInformations = new[]
                {
                    new DeploymentInformation
                    {
                        Method = DeploymentInformation.AzureDevOpsMethod,
                        Information = currentMethod.ToString()
                    }
                },
                Update = newMethod.ToString()
            }
        }.ToHttpContent();

        HttpResponseMessage? httpResponseMessage = null;

        try
        {
            httpResponseMessage = await _httpClient.PostAsync("managedeploymentinformation", httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.ToResponseObjectAsync<ManageDeploymentInformationResponse>();
        }
        catch (Exception ex)
        {
            throw new CmdbClientException(
                $"Error occurred in {nameof(UpdateDeploymentMethodAsync)}. " +
                $"endPoint='{httpResponseMessage?.RequestMessage?.RequestUri}'" +
                $"update='{newMethod}'", ex);
        }
    }

    private async Task<IEnumerable<CiContentItem>> GetAzDoCIsBatchLegacyAsync(string type, int start, int count)
    {
        var url = $"devices" +
                  $"?ConfigurationItemType={type}" +
                  $"&DeploymentMethod=Azure%20Devops" +
                  $"&start={start}" +
                  $"&count={count}" +
                  $"&view=expand";

        var httpResponseMessage = await _httpClient.GetAsync(url);

        httpResponseMessage.EnsureSuccessStatusCode();

        var responseObj = await httpResponseMessage.ToResponseObjectAsync<GetCiResponse>();
        return responseObj?.Content ?? Array.Empty<CiContentItem>();
    }

    private async Task<RetrieveCiByQueryResponse?> GetAzDoCIsBatchAsync(int start, int count)
    {
        var request = new RetrieveCiByQueryRequest
        {
            Body = new RetrieveCiByQueryRequestBody
            {
                StartNum = start,
                CountNum = count,
                Type = "retrievedeploymentinfo",
                Source = AzdoCompliancyCiName
            }
        };

        var httpResponseMessage = await _httpClient.PostAsync("retrieveCiInfoByQuery", request.ToHttpContent());

        httpResponseMessage.EnsureSuccessStatusCode();
        return await httpResponseMessage.ToResponseObjectAsync<RetrieveCiByQueryResponse>();
    }
}