using Microsoft.AspNetCore.Mvc;
using OrderProvider.Core.Interfaces.Services;
using OrderProvider.Core.Models;


namespace OrderProvider.Controllers;

[Route("api/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // Add a product to the cart
    [HttpPost("{cartId}/add")]
    public async Task<IActionResult> AddProductToCartAsync(Guid cartId, [FromBody] AddToCartRequest request)
    {
        try
        {
            await _cartService.AddProductToCartAsync(cartId, request);
            return Ok("Product added to cart successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error adding product to cart: {ex.Message}");
        }
    }

    // Update the cart with multiple products
    [HttpPut("{cartId}/update")]
    public async Task<IActionResult> UpdateCartAsync(Guid cartId, [FromBody] UpdateCartRequest request)
    {
        try
        {
            await _cartService.UpdateCartAsync(cartId, request);
            return Ok("Cart updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating cart: {ex.Message}");
        }
    }

    // Remove a product from the cart
    [HttpDelete("{cartId}/remove/{productId}")]
    public async Task<IActionResult> RemoveProductFromCartAsync(Guid cartId, Guid productId)
    {
        try
        {
            await _cartService.RemoveProductFromCartAsync(cartId, productId);
            return Ok("Product removed from cart successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error removing product from cart: {ex.Message}");
        }
    }
}
