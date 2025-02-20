using OrderProvider.Entities;
using OrderProvider.ServiceBus;

namespace OrderProvider.Interfaces.Services
{
    public interface IRabbitMqService
    {
        void PublishProductUpdate(List<ProductEntity> updatedProducts);
        void PublishInvoiceEvent(InvoiceEvent invoiceEvent);
        void PublishFileEvent(FileEvent fileEvent);
    }
}