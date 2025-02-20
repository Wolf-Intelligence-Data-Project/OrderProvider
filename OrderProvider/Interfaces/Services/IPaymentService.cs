using OrderProvider.Models;

namespace OrderProvider.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(Guid userId, decimal amount);
    }

}
