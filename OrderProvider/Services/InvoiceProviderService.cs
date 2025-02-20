using OrderProvider.Entities;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models;

namespace OrderProvider.Services
{
    public class InvoiceProviderService : IInvoiceProviderService
    {
        private readonly HttpClient _httpClient;

        public InvoiceProviderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendOrderToInvoiceProvider(OrderEntity order)
        {
            var request = new InvoiceRequestDto
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                CreatedAt = order.CreatedAt
            };

            await _httpClient.PostAsJsonAsync("https://invoiceprovider/api/invoice", request);
        }
    }

}
