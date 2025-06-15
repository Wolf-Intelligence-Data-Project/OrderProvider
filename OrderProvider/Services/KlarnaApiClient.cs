using OrderProvider.Entities;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Responses;

namespace OrderProvider.Services;

// Sandbox klarna 

public class KlarnaApiClient : IKlarnaApiClient
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<KlarnaApiClient> _logger;

    public KlarnaApiClient(IConfiguration configuration, HttpClient httpClient, ILogger<KlarnaApiClient> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<KlarnaPaymentResponse> CreatePaymentSessionAsync(OrderEntity order)
    {
        try
        {
            var apiUrl = _configuration.GetValue<string>("Klarna:ApiUrl");
            var apiKey = _configuration.GetValue<string>("Klarna:ApiKey");

            var paymentSessionRequest = new
            {
                order_id = order.OrderId.ToString(),
                amount = order.TotalPrice,
                currency = "SEK",
                customer_id = order.CustomerId.ToString(),
            };

            var response = await _httpClient.PostAsJsonAsync($"{apiUrl}/checkout/v1/orders", paymentSessionRequest);

            if (response.IsSuccessStatusCode)
            {
                var klarnaResponse = await response.Content.ReadAsAsync<KlarnaPaymentResponse>();
                return klarnaResponse; 
            }

            _logger.LogError("Failed to create Klarna payment session. Status Code: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Klarna API.");
            return null;
        }
    }
}
