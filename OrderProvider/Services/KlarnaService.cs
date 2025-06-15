using OrderProvider.Interfaces;
using OrderProvider.Interfaces.Repositories;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using OrderProvider.Interfaces.Services;
using Stripe;

namespace OrderProvider.Services;

// Sandbox klarna 

public class KlarnaService : IKlarnaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<KlarnaService> _logger;
    private readonly string _klarnaApiUrl;
    private readonly string _klarnaApiKey;
    private readonly string _klarnaApiPassword;
    private readonly string _fileProviderConnection;

    public KlarnaService(IOrderRepository orderRepository, HttpClient httpClient, IConfiguration configuration, ILogger<KlarnaService> logger)
    {
        _configuration = configuration;
        _klarnaApiKey = _configuration.GetValue<string>("Klarna:ApiKey");
        _klarnaApiUrl = _configuration.GetValue<string>("Klarna:ApiUrl");
        _klarnaApiPassword = _configuration.GetValue<string>("Klarna:ApiPassword");
        _fileProviderConnection = _configuration.GetValue<string>("ConnectionString:FileProvider");
        _orderRepository = orderRepository;
        _httpClient = httpClient;

        _logger = logger;
    }

    public async Task<string> CreatePaymentSessionAsync(Guid orderId, Guid customerId)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogError("Order not found: {OrderId}", orderId);
                return null;
            }

            decimal totalPriceWithoutVat = order.TotalPriceWithoutVat;
            decimal totalPrice = order.TotalPrice;
            int taxRate = 2500; // 25% tax rate in Klarna format (2500 = 25%)

            int unitPrice = (int)Math.Round(totalPriceWithoutVat * 100, MidpointRounding.AwayFromZero);
            int totalTaxAmount = (int)Math.Round(totalPrice * 0.16m * 100, MidpointRounding.AwayFromZero);
            int totalAmount = (int)Math.Round(unitPrice + (unitPrice * 0.25m), MidpointRounding.AwayFromZero);

            _logger.LogWarning("unitPrice: {UnitPrice}, totalTaxAmount: {TotalTaxAmount}, totalAmount: {TotalAmount}",
                unitPrice, totalTaxAmount, totalAmount);

            var orderLines = new[]
            {
            new
            {
                type = "digital",
                reference = "123456",
                name = "Test Product",
                quantity = 1,
                unit_price = unitPrice,
                total_amount = unitPrice,
                tax_rate = taxRate,
                total_tax_amount = totalTaxAmount
            }
        };

            var paymentSessionRequest = new
            {
                purchase_country = "SE",
                purchase_currency = "SEK",
                locale = "sv-SE",
                order_id = orderId.ToString(),
                order_tax_amount = totalTaxAmount,
                order_amount = unitPrice,
                order_lines = orderLines,
                customer = new
                {
                    email = order.CustomerEmail  // Customer email
                },
                merchant_urls = new
                {
                    confirmation = $"https://localhost:3000/confirmation?orderId={order.OrderId}",
                    push = $"https://localhost:7113/klarna-payment-status",
                    checkout = "http://localhost:3000/klarnacheckoutpage",
                    terms = "https://localhost:3000/terms"
                }
            };


            using (var client = new HttpClient())
            {
                // Step 3: Set up Klarna client with Basic Authentication
                client.BaseAddress = new Uri("https://api.playground.klarna.com");
                string credentials = $"{_klarnaApiKey}:{_klarnaApiPassword}";
                string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

                // Step 4: Serialize and send the Klarna payment session request
                var jsonPaymentContent = JsonConvert.SerializeObject(paymentSessionRequest);
                var content = new StringContent(jsonPaymentContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/checkout/v3/orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create Klarna payment session for CustomerId: {CustomerId}, StatusCode: {StatusCode}, Response: {ResponseContent}",
                        customerId, response.StatusCode, responseContent);
                    return null;
                }

                // Step 5: Parse the Klarna response to extract the checkout URL from the body
                var paymentSessionResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Klarna payment session response: {ResponseContent}", paymentSessionResponse); // Log the full response content

                var paymentSessionData = JsonConvert.DeserializeObject<dynamic>(paymentSessionResponse);

                // Extract the checkout URL from the response body
                string checkoutUrl = paymentSessionData?.merchant_urls?.checkout?.ToString();
                if (string.IsNullOrEmpty(checkoutUrl))
                {
                    _logger.LogError("No checkout URL found in Klarna response.");
                    return null;
                }

                // Log the extracted order_id and checkout URL
                _logger.LogInformation("Klarna payment session created for CustomerId: {CustomerId}, OrderId: {OrderId}, RedirectUrl: {CheckoutUrl}",
                    customerId, orderId, checkoutUrl);

                // Assuming you have an order object to update:


                // Log the extracted order_id and checkout URL
                _logger.LogInformation("Klarna payment session created for CustomerId: {CustomerId}, OrderId: {OrderId}, RedirectUrl: {CheckoutUrl}",
                    customerId, orderId, checkoutUrl);

                // Return the Klarna checkout URL for redirection
                return checkoutUrl;

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Klarna payment session for CustomerId: {CustomerId}", customerId);
            return null;
        }
    }
}
