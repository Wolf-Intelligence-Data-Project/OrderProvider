using OrderProvider.Entities;
using OrderProvider.Models.Requests;

namespace OrderProvider.Interfaces.Services;

public interface IOrderService
{
    Task<OrderEntity> CreateOrderAsync(PaymentRequest paymentRequest);

    Task<List<OrderEntity>> GetUserOrderHistoryAsync();
    Task<List<OrderEntity>> GetAllOrderHistoryAsync();

    Task RevertOrderAsync(Guid CustomerId, Guid OrderId);

    Task<bool> UpdatePaymentStatusAsync(string orderId, string paymentStatus, string transactionId);
}
