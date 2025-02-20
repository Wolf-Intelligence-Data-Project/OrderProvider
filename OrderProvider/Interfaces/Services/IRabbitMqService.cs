namespace OrderProvider.Interfaces.Services
{
    public interface IRabbitMqService
    {
        Task HandleEventAsync<T>(T @event) where T : class;
    }
}
