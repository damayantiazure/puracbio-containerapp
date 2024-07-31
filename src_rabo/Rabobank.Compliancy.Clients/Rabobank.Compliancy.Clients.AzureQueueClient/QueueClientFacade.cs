using Azure.Storage.Queues;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Clients.AzureQueueClient
{
    public class QueueClientFacade : IQueueClientFacade
    {
        private readonly QueueClient _queueClient;

        public QueueClientFacade(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public async Task SendMessageAsync<T>(T objectData) where T : class
        {
            var serializedData = JsonConvert.SerializeObject(objectData);
            await _queueClient.SendMessageAsync(serializedData);
        }
    }
}