using System;
using System.Threading.Tasks;

namespace OrderProvider.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(Guid userId);
        Task<bool> CompleteOrderAsync(Guid orderId);
    }
}
