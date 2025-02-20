using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(Guid userId, string filtersUsed);
        Task<List<OrderEntity>> GetUserOrderHistoryAsync(Guid userId);
        Task<List<OrderEntity>> GetAllOrderHistoryAsync();
    }
}
