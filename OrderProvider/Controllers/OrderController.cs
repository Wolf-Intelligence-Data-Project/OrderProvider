using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
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
            var orderId = await _orderService.CreateOrderAsync(orderRequest.UserId);
            return Ok(new { OrderId = orderId });
        }
    }
}
