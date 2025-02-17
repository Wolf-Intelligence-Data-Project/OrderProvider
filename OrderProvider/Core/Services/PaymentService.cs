using Newtonsoft.Json;
using OrderProvider.Core.Interfaces.Services;
using System.Text;

namespace OrderProvider.Core.Services;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;  // To get Klarna API keys from app settings

    public PaymentService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> CreateKlarnaPaymentSessionAsync(Guid orderId, decimal amount)
    {
        var apiUrl = _configuration["Klarna:ApiUrl"]; // Base URL for Klarna API
        var apiKey = _configuration["Klarna:ApiKey"]; // API Key for Klarna

        // Construct request body for Klarna
        var requestBody = new
        {
            order_id = orderId.ToString(),
            amount,
            currency = "SEK",
            capture_mode = "AUTOMATIC",  // Klarna payment mode
            locale = "sv_SE" // Swedish locale
        };

        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
        };

        // Add Klarna API Key to the Authorization header
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to create Klarna payment session");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var sessionId = JsonConvert.DeserializeObject<dynamic>(responseContent).session_id;

        return sessionId; // Klarna session ID, which can be used for redirection
    }

    public async Task<bool> ConfirmKlarnaPaymentAsync(Guid orderId)
    {
        var apiUrl = _configuration["Klarna:ApiUrl"];
        var apiKey = _configuration["Klarna:ApiKey"];

        var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}/{orderId}/status")
        {
            Headers = { { "Authorization", $"Bearer {apiKey}" } }
        };

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to confirm Klarna payment");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var paymentStatus = JsonConvert.DeserializeObject<dynamic>(responseContent).status;

        return paymentStatus == "PAID";  // Check if the payment is successful
    }
}