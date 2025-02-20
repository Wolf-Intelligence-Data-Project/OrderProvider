namespace OrderProvider.Interfaces.Repositories
{
    public interface IRabbitMQService
    {
        void PublishEvent<T>(T eventMessage);
    }
}
