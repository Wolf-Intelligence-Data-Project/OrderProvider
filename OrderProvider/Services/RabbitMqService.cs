using System.Text;
using Newtonsoft.Json;
using OrderProvider.Interfaces.Repositories;
using RabbitMQ.Client;

namespace OrderProvider.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQService()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        public void PublishEvent<T>(T eventMessage)
        {
            // Ensure that the connection and channel are created properly
            using var connection = _factory.CreateConnection();  // Creates a connection to RabbitMQ
            using var channel = connection.CreateModel();        // Creates a channel for publishing messages

            // Declare the queue to make sure it exists
            channel.QueueDeclare(queue: "order_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Serialize the message into JSON
            var message = JsonConvert.SerializeObject(eventMessage);
            var body = Encoding.UTF8.GetBytes(message);  // Convert the message into a byte array

            // Publish the message to the queue
            channel.BasicPublish(exchange: "", routingKey: "order_queue", basicProperties: null, body: body);
        }
    }
}
