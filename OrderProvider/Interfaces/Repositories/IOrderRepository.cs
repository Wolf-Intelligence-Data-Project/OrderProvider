using System;
using System.Threading.Tasks;
using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderEntity> GetOrderByIdAsync(Guid orderId);
        Task<Guid> CreateOrderAsync(OrderEntity order);
    }
}
