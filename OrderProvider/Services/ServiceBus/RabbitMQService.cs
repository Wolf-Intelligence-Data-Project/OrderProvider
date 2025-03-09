using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace OrderProvider.Services.ServiceBus;

public class RabbitMQService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _hostName;

    public RabbitMQService(IConfiguration configuration)
    {
        _hostName = configuration.GetValue<string>("RabbitMQ:HostName"); // URL from appsettings.json
        var factory = new ConnectionFactory() { HostName = _hostName };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task SendMessageAsync(string message, string queueName)
    {
        _channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        await Task.CompletedTask;
    }

    public async Task<string> ReceiveMessageAsync(string queueName)
    {
        var message = string.Empty;

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            message = Encoding.UTF8.GetString(body);
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        // Wait for the message to be received
        await Task.Delay(500); // Adjust delay as per your system's message processing time
        return message;
    }

    // Close the connection and channel properly
    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}
