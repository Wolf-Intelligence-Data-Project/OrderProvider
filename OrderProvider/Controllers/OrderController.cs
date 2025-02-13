using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderProvider.Services;

namespace OrderProvider.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] Cart cart)
    {
        var order = await _orderService.PlaceOrderAsync(cart.Id);
        return Ok(order);
    }
}
