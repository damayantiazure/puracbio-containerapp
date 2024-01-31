using Newtonsoft.Json;
using Rabobank.Compliancy.Domain.Compliancy.Rules;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using System.Net.Mime;
using System.Text;

namespace Rabobank.Compliancy.Infra.Sm9Client;

internal static class ItsmClientExtensions
{
    internal static async Task<T?> ToResponseObjectAsync<T>(this HttpResponseMessage httpResponseMessage)
    {
        var json = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    internal static HttpContent ToHttpContent(this object content) =>
        new StringContent(JsonConvert.SerializeObject(content),
            Encoding.UTF8, MediaTypeNames.Application.Json);

    internal static IEnumerable<CiContentItem> ToCiContentItems(this IEnumerable<RetrieveCiByQueryResponseInformation> responseInformationList)
    {
        var ciContentItems = new List<CiContentItem>();

        foreach (var responseInformation in responseInformationList)
        {
            var ciContentItem = new CiContentItem
            {
                Device = new ConfigurationItemModel
                {
                    AssignmentGroup = responseInformation.ConfigAdminGroup,
                    BIVcode = responseInformation.AicClassification,
                    CiIdentifier = responseInformation.CiID,
                    ConfigurationItem = responseInformation.CiName,
                    ConfigurationItemType = responseInformation.CiType,
                    ConfigurationItemSubType = responseInformation.CiSubtype,
                    DisplayName = responseInformation.CiName,
                    Environment = responseInformation.Environment,
                    SOXClassification = responseInformation.SoxClassification,
                    Status = responseInformation.Status,
                    DeploymentInfo = responseInformation.DeploymentInfo,
                }
            };
            ciContentItems.Add(ciContentItem);
        }
        return ciContentItems;
    }

}