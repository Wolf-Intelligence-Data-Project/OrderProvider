using OrderProvider.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProvider.Core.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrdersByCompanyIdAsync(int companyId);
        Task<bool> UpdateOrderPaymentStatusAsync(int orderId, string paymentStatus);
    }
}
