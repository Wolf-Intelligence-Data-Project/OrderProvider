using OrderProvider.Entities;
using OrderProvider.Models.Responses;
using OrderProvider.Services;

namespace OrderProvider.Interfaces.Services;

// Sandbox klarna 
public interface IKlarnaApiClient
{
    Task<KlarnaPaymentResponse> CreatePaymentSessionAsync(OrderEntity order);
}
