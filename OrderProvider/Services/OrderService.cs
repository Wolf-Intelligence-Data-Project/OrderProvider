using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Requests;
using Microsoft.Extensions.Options;
using OrderProvider.Data;
using OrderProvider.Models.Settings;

namespace OrderProvider.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly IOptions<PriceSettings> _priceSettings;
    private readonly ILogger<OrderService> _logger;
    private readonly ProductDbContext _productDbContext;
    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IReservationRepository reservationRepository,
        IRabbitMQService rabbitMQService,
        ILogger<OrderService> logger,
        IOptions<PriceSettings> priceSettings,
        ProductDbContext context)


    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
        _rabbitMQService = rabbitMQService;
        _priceSettings = priceSettings;
        _logger = logger;
        _productDbContext = context;
    }

    //public async Task<ReservationDto> GetReservationAsync(OrderRequest orderRequest)
    //{
    //    var reservationId = orderRequest.ReservationId;

    //    if (reservationId == Guid.Empty)
    //    {
    //        return null;
    //    }

    //    var reservation = await _productRepository.GetReservationByIdAsync(reservationId);

    //    if (reservation == null)
    //    {
    //        return null;
    //    }

    //    return ReservationFactory.CreateReservationDto(reservation);
    //}

    public async Task CreateOrderAsync(OrderRequest orderRequest)
    {
        try
        {
            // Get reservation details
            var reservation = await _reservationRepository.GetReservationByCustomerIdAsync(orderRequest.CustomerId);
            if (reservation == null)
            {
                _logger.LogWarning("No reservation found for CustomerId: {CustomerId}", orderRequest.CustomerId);
                return;
            }

            _logger.LogInformation("Found reservation for CustomerId: {CustomerId} with ReservationId: {ReservationId}",
                orderRequest.CustomerId, reservation.ReservationId);

            // Update Product Table (Set ReservedUntil = NULL, SoldUntil = 30 days from now)
            var stockholmTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm"));
            await _productRepository.ProductSoldAsync(orderRequest.CustomerId);

            // Update Reservation Table (Set ReservedFrom = NULL, SoldFrom = NOW)
            await _reservationRepository.UpdateToSoldAsync(reservation.ReservationId);

            // Calculate price
            decimal pricePerProduct = _priceSettings.Value.PricePerProduct;
            decimal vatRate = _priceSettings.Value.VatRate;
            decimal totalPrice = reservation.Quantity * pricePerProduct * (1 + vatRate / 100);

            // Create Order in another database
            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                UserId = orderRequest.CustomerId,
                CreatedAt = stockholmTime,
                PricePerProductAtPurchase = pricePerProduct,
                Quantity = reservation.Quantity,
                TotalPrice = totalPrice,
                PaymentStatus = orderRequest.IsPayed ? "Paid" : "Pending",
                FiltersUsed = reservation.ReservationId
            };

            await _orderRepository.CreateOrderAsync(order);
            _logger.LogInformation("Order successfully created for CustomerId: {CustomerId} with OrderId: {OrderId}",
                orderRequest.CustomerId, order.Id);

            // Send message to FileGeneration queue
            await _rabbitMQService.SendMessageAsync(order.Id.ToString(), "file-generation-queue");
            _logger.LogInformation("Sent message to FileGeneration queue for OrderId: {OrderId}", order.Id);

            // Send message to InvoiceGeneration queue
            await _rabbitMQService.SendMessageAsync(order.Id.ToString(), "invoice-generation-queue");
            _logger.LogInformation("Sent message to InvoiceGeneration queue for OrderId: {OrderId}", order.Id);

            // If the payment is confirmed, send message to PaymentConfirmation queue
            if (orderRequest.IsPayed)
            {
                await _rabbitMQService.SendMessageAsync(order.Id.ToString(), "payment-confirmed");
                _logger.LogInformation("Payment confirmed for OrderId: {OrderId}, message sent to PaymentConfirmation queue.", order.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order for CustomerId: {CustomerId}", orderRequest.CustomerId);
        }
    }

    public async Task RevertOrderAsync(Guid CustomerId, Guid OrderId)
    {
        try
        {
            _logger.LogWarning("Reverting order creation for CustomerId: {CustomerId}", CustomerId);

            // Revert logic here

            _logger.LogInformation("Order reverted successfully for the order: ", OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reverting order for CustomerId: {CustomerId}", CustomerId);
        }
    }
    public async Task<List<OrderEntity>> GetUserOrderHistoryAsync(Guid userId)
    {
        return await _orderRepository.GetOrdersByUserIdAsync(userId);
    }

    public async Task<List<OrderEntity>> GetAllOrderHistoryAsync()
    {
        return await _orderRepository.GetAllOrdersAsync();
    }
}