using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Services;

public interface IInvoiceProviderService
{
    Task SendOrderToInvoiceProvider(OrderEntity order);
}