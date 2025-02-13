using Azure.Messaging.ServiceBus;

namespace OrderProvider.Queues;

public class QueueService
{
    private readonly ServiceBusClient _client;

    public QueueService(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task SendMessageAsync(string message)
    {
        var sender = _client.CreateSender("<queue_name>");
        var serviceBusMessage = new ServiceBusMessage(message);
        await sender.SendMessageAsync(serviceBusMessage);
    }
}
