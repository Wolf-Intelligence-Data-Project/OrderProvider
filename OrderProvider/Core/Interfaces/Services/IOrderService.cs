using OrderProvider.Core.DTOs;
using OrderProvider.Core.Entities;
using System.Threading.Tasks;

namespace OrderProvider.Core.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderRequest request);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<bool> ConfirmPaymentAsync(int orderId);
    }
}
