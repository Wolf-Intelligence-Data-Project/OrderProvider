using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderProvider.Messaging.AzureServiceBus
{
    public class AzureServiceBusPublisher : IAzureServiceBusPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _queueName = "bulk-product-updates"; // Adjust queue name as necessary

        public AzureServiceBusPublisher(ServiceBusClient client)
        {
            _client = client;
        }

        public async Task PublishAsync<T>(T message)
        {
            var sender = _client.CreateSender(_queueName);
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
