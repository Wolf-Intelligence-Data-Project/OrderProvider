using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderEntity> GetOrderByIdAsync(Guid orderId);
        Task<List<OrderEntity>> GetOrdersByUserIdAsync(Guid userId);  
        Task<List<OrderEntity>> GetAllOrdersAsync();
        Task<bool> DeleteOrderAsync(Guid userId);

        Task<bool> DeleteUnpaidOrdersAsync(Guid userId);

        Task CreateOrderAsync(OrderEntity order);

        Task<OrderEntity> UpdateOrderAsync(OrderEntity order);
    }

}

