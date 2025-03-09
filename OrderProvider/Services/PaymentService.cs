using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Responses;

namespace OrderProvider.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(Guid userId, decimal amount)
        {
            //var response = await _httpClient.PostAsJsonAsync("https://klarna/api/payments", new
            //{
            //    UserId = userId,
            //    Amount = amount
            //});

            //return await response.Content.ReadFromJsonAsync<PaymentResponse>();
            return new PaymentResponse();
        }
    }

}
