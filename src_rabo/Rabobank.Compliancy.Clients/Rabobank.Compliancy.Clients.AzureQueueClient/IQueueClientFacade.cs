namespace Rabobank.Compliancy.Clients.AzureQueueClient
{
    /// <summary>
    /// Used as a 'wrapper' interface to be able to make use of DI.
    /// </summary>
    public interface IQueueClientFacade
    {
        Task SendMessageAsync<T>(T objectData) where T : class;
    }
}
