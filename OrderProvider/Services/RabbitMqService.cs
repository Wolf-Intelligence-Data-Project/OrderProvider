using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using OrderProvider.Entities;
using OrderProvider.Interfaces.Services;
using OrderProvider.ServiceBus;
using RabbitMQ.Client;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OrderProvider.Services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqService()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        // Method specifically for product updates
        public void PublishProductUpdate(List<ProductEntity> updatedProducts)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "product_updates", durable: true, exclusive: false, autoDelete: false);

            var message = JsonSerializer.Serialize(updatedProducts);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: "product_updates", basicProperties: null, body: body);
        }

        public void PublishInvoiceEvent(InvoiceEvent invoiceEvent)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "invoice_provider", durable: true, exclusive: false, autoDelete: false);

            var message = JsonSerializer.Serialize(invoiceEvent);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: "invoice_provider", basicProperties: null, body: body);
        }

        public void PublishFileEvent(FileEvent fileEvent)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "file_provider", durable: true, exclusive: false, autoDelete: false);

            var message = JsonSerializer.Serialize(fileEvent);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: "file_provider", basicProperties: null, body: body);
        }
    }
}
