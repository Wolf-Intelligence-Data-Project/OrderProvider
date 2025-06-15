using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Requests;
using Microsoft.Extensions.Options;
using OrderProvider.Data;
using OrderProvider.Models.Settings;
using OrderProvider.Interfaces.Helpers;
using System.Text;
using System.Text.Json;

namespace OrderProvider.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ITokenExtractor _tokenExtractor;
    private readonly IConfiguration _configuration;
    private readonly IOptions<PriceSettings> _priceSettings;
    private readonly ILogger<OrderService> _logger;
    private readonly ProductDbContext _productDbContext;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IReservationRepository reservationRepository,
        ITokenExtractor tokenExtractor,
        IConfiguration configuration,
        ILogger<OrderService> logger,
        IOptions<PriceSettings> priceSettings,
        ProductDbContext context)


    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
        _tokenExtractor = tokenExtractor;
        _configuration = configuration;
        _priceSettings = priceSettings;
        _logger = logger;
        _productDbContext = context;
    }
    private async Task<bool> PaymentTemporary(PaymentRequest paymentRequest)
    {
        if (paymentRequest == null)
            return false;

        // Mock card

        var testCard = new PaymentRequest
        {
            CardNumber = "1234123412341234",
            CardExpiration = "1235",
            CVV = "123"
        };

        bool isValid =
            paymentRequest.CardNumber?.Replace(" ", "") == testCard.CardNumber &&  
            paymentRequest.CardExpiration?.Replace(" ", "") == testCard.CardExpiration &&  
            paymentRequest.CVV == testCard.CVV;

        return await Task.FromResult(isValid);
    }

    public async Task<OrderEntity> CreateOrderAsync(PaymentRequest paymentRequest)
    {
        var customerId = _tokenExtractor.GetUserIdFromToken();

        if (customerId == null)
        {
            _logger.LogWarning("Invalid or missing userId in cookie.");
            return null;
        }

        try
        {
            _logger.LogInformation("Payment Request received: CardNumber={CardNumber}, CardExpiration={CardExpiration}, CVV={CVV}",
            paymentRequest.CardNumber, paymentRequest.CardExpiration, paymentRequest.CVV);

            if (await PaymentTemporary(paymentRequest) != true)
            {
                _logger.LogError("Payment information is missing or invalid.");
                return null;
            }

            string paymentStatus = "Payed";

            var reservation = await _reservationRepository.GetReservationAsync(customerId);
            if (reservation == null)
            {
                _logger.LogWarning("No reservation found for CustomerId: {CustomerId}", customerId);
                return null;
            }

            _logger.LogInformation("Found reservation for CustomerId: {CustomerId} with ReservationId: {ReservationId}",
                customerId, reservation.ReservationId);

            await _productRepository.ProductSoldAsync(customerId);

            int quantity = reservation.Quantity;
            decimal pricePerProduct = 6;
            decimal vatRate = 0.25m;
            decimal totalPriceWithoutVat = quantity * pricePerProduct;
            decimal totalPrice = totalPriceWithoutVat + (totalPriceWithoutVat * vatRate);

            var order = new OrderEntity
            {
                OrderId = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerEmail = "dkomnenovic@ymail.com",
                CreatedAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm")),
                PricePerProduct = pricePerProduct,
                Quantity = quantity,
                TotalPrice = totalPrice,
                TotalPriceWithoutVat = totalPriceWithoutVat,
                PaymentStatus = paymentStatus,
                FiltersUsed = reservation.ReservationId
            };

            await _orderRepository.CreateOrderAsync(order);
            _logger.LogInformation("Order successfully created for CustomerId: {CustomerId} with OrderId: {OrderId}",
                customerId, order.OrderId);

            await _reservationRepository.DeleteReservationImmediatelyAsync(reservation.ReservationId);

            await SendOrderToAzureFunctionAsync(order);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order for CustomerId: {CustomerId}", customerId);
            return null;
        }
    }

    private async Task SendOrderToAzureFunctionAsync(OrderEntity order)
    {
        using (var client = new HttpClient())
        {
            var url = "http://localhost:7223/api/order-report";  // Local Azure Function URL
            var jsonContent = JsonSerializer.Serialize(order);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order sent to Azure function successfully.");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to send order. Status: {response.StatusCode}. Content: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order to Azure function.");
            }
        }
    }


    public async Task<bool> UpdatePaymentStatusAsync(string orderId, string paymentStatus, string klarnaPaymentId)
    {
        try
        {
            if (Guid.TryParse(orderId, out var orderGuid))
            {
                // Fetching the order from the database by the Klarna OrderId (Guid)
                var order = await _orderRepository.GetOrderByIdAsync(orderGuid);
                if (order == null)
                {
                    _logger.LogWarning("Order not found for OrderId: {OrderId}", orderId);
                    return false;
                }

                order.PaymentStatus = paymentStatus;
                order.KlarnaPaymentId = klarnaPaymentId; 

                await _orderRepository.UpdateOrderAsync(order);

                _logger.LogInformation("Payment status for OrderId: {OrderId} updated to {PaymentStatus}. Klarna PaymentId: {KlarnaPaymentId}.", orderId, paymentStatus, klarnaPaymentId);
                return true;
            }
            else
            {
                _logger.LogWarning("Invalid OrderId format: {OrderId}", orderId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status for OrderId: {OrderId}", orderId);
            return false;
        }
    }


    public async Task RevertOrderAsync(Guid CustomerId, Guid OrderId)
    {
        try
        {
            _logger.LogWarning("Reverting order creation for CustomerId: {CustomerId}", CustomerId);

            // Placeholder for revert logic

            _logger.LogInformation("Order reverted successfully for the order: ", OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reverting order for CustomerId: {CustomerId}", CustomerId);
        }
    }
    public async Task<List<OrderEntity>> GetUserOrderHistoryAsync()
    {
        var userId = _tokenExtractor.GetUserIdFromToken();

        if (userId == null)
        {
            _logger.LogWarning("Invalid or missing userId in cookie.");
            return null;
        }
        return await _orderRepository.GetOrdersByUserIdAsync(userId);
    }

    public async Task<List<OrderEntity>> GetAllOrderHistoryAsync()
    {
        return await _orderRepository.GetAllOrdersAsync();
    }
}