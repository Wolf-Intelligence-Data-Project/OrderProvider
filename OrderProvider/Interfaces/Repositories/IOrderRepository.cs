using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderEntity> GetOrderByIdAsync(Guid orderId);
        Task<Guid> CreateOrderAsync(OrderEntity order);
        Task<List<OrderEntity>> GetOrdersByUserIdAsync(Guid userId);  // Added this method
        Task<List<OrderEntity>> GetAllOrdersAsync();  // Added this method
    }
}
