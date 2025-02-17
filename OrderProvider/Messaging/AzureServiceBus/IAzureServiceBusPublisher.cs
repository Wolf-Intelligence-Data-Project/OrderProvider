namespace OrderProvider.Messaging.AzureServiceBus
{
    public interface IAzureServiceBusPublisher
    {
        Task PublishAsync<T>(T message);
    }
}
