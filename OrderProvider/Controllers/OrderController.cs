using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Requests;


namespace OrderProvider.Controllers;

[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderController> _logger;
    private readonly IReservationRepository _reservationRepository;
    public OrderController(IOrderService orderService, IOrderRepository orderRepository, ILogger<OrderController> logger, IReservationRepository reservationRepository)
    {
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
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
    {
        if (orderRequest == null)
        {
            _logger.LogWarning("Received invalid order request: OrderRequest is null.");
            return BadRequest("Invalid order request.");
        }

        try
        {
            await _orderService.CreateOrderAsync(orderRequest);
            _logger.LogInformation("Order created successfully for CustomerId: {CustomerId}", orderRequest.CustomerId);
            return Ok("Order created successfully.");
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError(argEx, "Invalid argument while processing order for CustomerId: {CustomerId}", orderRequest.CustomerId);
            return BadRequest("Invalid order data provided.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the order for CustomerId: {CustomerId}", orderRequest.CustomerId);
            return StatusCode(500, "An error occurred while processing the order.");
        }
    }

    [HttpPost("delete-order")]
    public async Task<IActionResult> DeleteOrder(Guid UserId)
    {
        var orderId = await _orderRepository.DeleteOrderAsync(UserId);
        return Ok();
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetUserOrderHistory(Guid userId)
    {
        var orders = await _orderService.GetUserOrderHistoryAsync(userId);
        return Ok(orders);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetAllOrderHistory()
    {
        var orders = await _orderService.GetAllOrderHistoryAsync();
        return Ok(orders);
    }
}
