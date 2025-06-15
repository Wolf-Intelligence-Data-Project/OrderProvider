using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderProvider.Interfaces;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Requests;
using OrderProvider.Models.Responses;

namespace OrderProvider.Controllers;
[Authorize]
[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IKlarnaService _klarnaService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderController> _logger;
    private readonly IReservationRepository _reservationRepository;
    public OrderController(IOrderService orderService, IKlarnaService klarnaService, IOrderRepository orderRepository, ILogger<OrderController> logger, IReservationRepository reservationRepository)
    {
        _klarnaService = klarnaService;
        _orderService = orderService;
        _orderRepository = orderRepository;
        _logger = logger;
        _reservationRepository = reservationRepository;
    }

    [HttpDelete("delete-sold-products")]
    public async Task<IActionResult> DeleteSoldProducts()
    {
        try
        {
            // Call the repository method to delete products
            var rowsAffected = await _reservationRepository.DeleteSoldProductsAsync();

            if (rowsAffected > 0)
            {
                return Ok(new { message = $"{rowsAffected} product(s) deleted successfully." });
            }
            else
            {
                return NotFound(new { message = "No products with non-null SoldUntil found." });
            }
        }
        catch (Exception ex)
        {
            // Handle exception (e.g., log error)
            return StatusCode(500, new { message = "An error occurred while deleting products.", error = ex.Message });
        }
    }
    [HttpPost("order")]
    public async Task<IActionResult> CreateOrder([FromBody] PaymentRequest paymentRequest)
    {
        if (paymentRequest == null)
        {
            _logger.LogWarning("Received invalid order request: OrderRequest is null.");
            return BadRequest("Invalid order request.");
        }

        try
        {
            // Create the order
            var order = await _orderService.CreateOrderAsync(paymentRequest);

            if (order == null)
            {
                _logger.LogError("Failed to create order.");
                return StatusCode(500, "Failed to create order.");
            }
            var orderId = order.OrderId; // Replace 'Id' with the actual property name that holds the order ID.
            var customerId = order.CustomerId; // Replace 'CustomerId' with the actual property name for the customer ID.
            var totalPriceWithoutVat = order.TotalPriceWithoutVat; // Replace 'TotalPrice' with the actual property name for the total price.

            var klarnaPaymentResponse = await _klarnaService.CreatePaymentSessionAsync(orderId, customerId);

            if (klarnaPaymentResponse == null || string.IsNullOrEmpty(klarnaPaymentResponse))
            {
                _logger.LogError("Klarna payment session creation failed.");
                return StatusCode(500, "Failed to create Klarna payment session.");
            }

            _logger.LogInformation("Order created successfully for CustomerId: {CustomerId}, redirect to Klarna payment URL.", customerId);
            return Ok(new { klarnaPaymentResponse });
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError(argEx, "Invalid argument while processing order.");
            return BadRequest("Invalid order data provided.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the order.");
            return StatusCode(500, "An error occurred while processing the order.");
        }
    }


    [HttpPost("delete-order")]
    public async Task<IActionResult> DeleteOrder(Guid UserId)
    {

        var orderId = await _orderRepository.DeleteOrderAsync(UserId);
        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetUserOrderHistory()
    {
        var orders = await _orderService.GetUserOrderHistoryAsync();
        return Ok(orders);
    }

    //[HttpGet("history")]
    //public async Task<IActionResult> GetAllOrderHistory()
    //{
    //    var orders = await _orderService.GetAllOrderHistoryAsync();
    //    return Ok(orders);
    //}



    //[HttpPost("klarna-webhook")]
    //public async Task<IActionResult> KlarnaWebhook([FromBody] KlarnaWebhookRequest webhookRequest)
    //{
    //    if (webhookRequest == null)
    //    {
    //        _logger.LogWarning("Received invalid Klarna webhook: WebhookRequest is null.");
    //        return BadRequest("Invalid webhook request.");
    //    }

    //    try
    //    {
    //        _logger.LogInformation("Received Klarna webhook for OrderId: {OrderId}", webhookRequest.OrderId);

    //        // Handle Klarna payment status update
    //        var paymentStatus = webhookRequest.Status;
    //        if (paymentStatus == "paid")
    //        {
    //            // Update payment status in the order system
    //            await _orderService.UpdatePaymentStatusAsync(webhookRequest.OrderId, "Paid");
    //            _logger.LogInformation("Payment confirmed for OrderId: {OrderId}", webhookRequest.OrderId);

    //            // Send message to FileGeneration queue to generate the file
    //            await _rabbitMQService.SendMessageAsync(webhookRequest.OrderId.ToString(), "file-generation-queue");
    //        }
    //        else if (paymentStatus == "pending")
    //        {
    //            // Handle pending status
    //            _logger.LogInformation("Payment pending for OrderId: {OrderId}", webhookRequest.OrderId);
    //        }
    //        else
    //        {
    //            _logger.LogWarning("Unknown payment status for OrderId: {OrderId}", webhookRequest.OrderId);
    //        }

    //        return Ok("Webhook handled successfully.");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing Klarna webhook for OrderId: {OrderId}", webhookRequest.OrderId);
    //        return StatusCode(500, "Error processing Klarna webhook.");
    //    }
    //}

    [HttpPost("klarna-payment-status")]
    public async Task<IActionResult> KlarnaPaymentStatusWebhook([FromBody] KlarnaPaymentStatusWebhookRequest webhookRequest)
    {
        if (webhookRequest == null)
        {
            _logger.LogWarning("Received invalid Klarna payment status webhook: Request is null.");
            return BadRequest("Invalid webhook request.");
        }

        try
        {
            _logger.LogInformation("Received Klarna payment status webhook for OrderId: {OrderId}, Status: {Status}", webhookRequest.OrderId, webhookRequest.Status);

            var transactionId = webhookRequest.TransactionId;
            var paymentStatus = webhookRequest.Status;

            if (paymentStatus == "paid")
            {
                await _orderService.UpdatePaymentStatusAsync(webhookRequest.OrderId, "Paid", transactionId);
                _logger.LogInformation("Payment confirmed for OrderId: {OrderId}, KlarnaOrderId: {KlarnaOrderId}", webhookRequest.OrderId, transactionId);
            }
            else if (paymentStatus == "pending")
            {
                _logger.LogInformation("Payment pending for OrderId: {OrderId}", webhookRequest.OrderId);
            }
            else if (paymentStatus == "failed")
            {
                _logger.LogWarning("Payment failed for OrderId: {OrderId}", webhookRequest.OrderId);
            }
            else
            {
                _logger.LogWarning("Unknown payment status for OrderId: {OrderId}", webhookRequest.OrderId);
            }

            return Ok("Webhook handled successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Klarna payment status webhook for OrderId: {OrderId}", webhookRequest.OrderId);
            return StatusCode(500, "Error processing Klarna payment status webhook.");
        }
    }


}
