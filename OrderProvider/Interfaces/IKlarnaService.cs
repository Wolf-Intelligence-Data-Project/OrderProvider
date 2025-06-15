using OrderProvider.Models.Responses;

namespace OrderProvider.Interfaces;

public interface IKlarnaService
{

    Task<string> CreatePaymentSessionAsync(Guid orderId, Guid customerId);

}