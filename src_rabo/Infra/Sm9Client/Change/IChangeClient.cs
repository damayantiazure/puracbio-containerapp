using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

namespace Rabobank.Compliancy.Infra.Sm9Client.Change;

public interface IChangeClient
{
    Task<CreateChangeResponse?> CreateChangeAsync(CreateChangeRequestBody requestBody);
    Task<CloseChangeResponse?> CloseChangeAsync(CloseChangeRequestBody requestBody);        
    Task<UpdateChangeResponse?> UpdateChangeAsync(UpdateChangeRequestBody requestBody);
    Task<GetChangeByKeyResponse?> GetChangeByKeyAsync(GetChangeByKeyRequestBody requestBody);
}