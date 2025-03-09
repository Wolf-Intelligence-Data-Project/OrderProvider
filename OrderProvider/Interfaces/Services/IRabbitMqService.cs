namespace OrderProvider.Interfaces.Services;

public interface IRabbitMQService
{
    Task SendMessageAsync(string message, string queueName);
    Task<string> ReceiveMessageAsync(string queueName);
}