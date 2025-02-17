using System;
using System.Threading.Tasks;

namespace OrderProvider.Core.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<bool> CreatePaymentSessionAsync(Guid orderId, decimal amount);
        Task<bool> ConfirmPaymentAsync(Guid orderId);
        Task SavePaymentDetailsAsync(Guid orderId, string sessionId, string status);
    }
}
