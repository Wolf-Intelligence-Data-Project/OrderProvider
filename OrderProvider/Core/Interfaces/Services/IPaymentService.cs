using System;
using System.Threading.Tasks;

namespace OrderProvider.Core.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<string> CreateKlarnaPaymentSessionAsync(Guid orderId, decimal amount);
        Task<bool> ConfirmKlarnaPaymentAsync(Guid orderId);
    }
}
