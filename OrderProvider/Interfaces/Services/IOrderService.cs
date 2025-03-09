using OrderProvider.Entities;
using OrderProvider.Models.DTOs;
using OrderProvider.Models.Requests;

namespace OrderProvider.Interfaces.Services
{
    public interface IOrderService
    {
        //Task<ReservationDto> GetReservationAsync(OrderRequest orderRequest);
        Task CreateOrderAsync(OrderRequest orderRequest);
        Task<List<OrderEntity>> GetUserOrderHistoryAsync(Guid userId);
        Task<List<OrderEntity>> GetAllOrderHistoryAsync();

        Task RevertOrderAsync(Guid CustomerId, Guid OrderId);
    }
}
