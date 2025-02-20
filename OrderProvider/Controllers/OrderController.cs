using Microsoft.AspNetCore.Mvc;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models;

namespace OrderProvider.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto orderRequest)
        {
            var orderId = await _orderService.CreateOrderAsync(orderRequest.UserId, orderRequest.FiltersUsed);
            return Ok(new { OrderId = orderId });
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
}
