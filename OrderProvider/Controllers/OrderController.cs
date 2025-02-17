using Microsoft.AspNetCore.Mvc;
using OrderProvider.Core.DTOs;
using OrderProvider.Core.Interfaces.Services;
using System.Threading.Tasks;

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

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var order = await _orderService.CreateOrderAsync(request);
            return Ok(order);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost("{orderId}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(int orderId)
        {
            var success = await _orderService.ConfirmPaymentAsync(orderId);
            if (!success) return BadRequest("Payment confirmation failed.");
            return Ok("Payment confirmed.");
        }
    }
}
