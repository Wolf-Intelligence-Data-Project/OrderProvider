using OrderProvider.Core.Entities;
using OrderProvider.Core.Interfaces.Repositories;
using OrderProvider.Core.Interfaces.Services;
using OrderProvider.Core.Models;

namespace OrderProvider.Core.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IBulkProductUpdateService _bulkProductUpdateService;

    public CartService(ICartRepository cartRepository, IBulkProductUpdateService bulkProductUpdateService)
    {
        _cartRepository = cartRepository;
        _bulkProductUpdateService = bulkProductUpdateService;
    }

    public async Task AddProductToCartAsync(Guid cartId, AddToCartRequest request)
    {
        // Fetch the cart from OrderDatabase
        var cart = await _cartRepository.GetByIdAsync(cartId);
        if (cart == null)
        {
            throw new Exception("Cart not found.");
        }

        // Ensure only unique products are added (no duplicates)
        if (!cart.CartItems.Any(item => item.ProductId == request.ProductId))
        {
            cart.CartItems.Add(new CartItemEntity { ProductId = request.ProductId });
        }

        // Update the ReservedUntil field for the product using BulkProductUpdateService
        await _bulkProductUpdateService.ReserveProductsAsync(new List<Guid> { request.ProductId });

        // Set the expiration time (15 minutes)
        cart.ExpirationTime = DateTime.UtcNow.AddMinutes(15);
        cart.LastUpdated = DateTime.UtcNow;

        // Save the cart in OrderDatabase
        await _cartRepository.SaveAsync(cart);
    }

    public async Task UpdateCartAsync(Guid cartId, UpdateCartRequest request)
    {
        // Fetch the cart from OrderDatabase
        var cart = await _cartRepository.GetByIdAsync(cartId);
        if (cart == null)
        {
            throw new Exception("Cart not found.");
        }

        // Remove products that are not in the updated list
        var itemsToRemove = cart.CartItems.Where(item => !request.ProductIds.Contains(item.ProductId)).ToList();
        foreach (var item in itemsToRemove)
        {
            cart.CartItems.Remove(item);
        }

        // Add the new products, ensuring they are unique (1 product = 1 cart item)
        foreach (var productId in request.ProductIds)
        {
            if (!cart.CartItems.Any(i => i.ProductId == productId))
            {
                cart.CartItems.Add(new CartItemEntity { ProductId = productId });
            }
        }

        // Update ReservedUntil for the updated products
        await _bulkProductUpdateService.ReserveProductsAsync(request.ProductIds);

        // Set the expiration time (15 minutes)
        cart.ExpirationTime = DateTime.UtcNow.AddMinutes(15);
        cart.LastUpdated = DateTime.UtcNow;

        // Save the updated cart in OrderDatabase
        await _cartRepository.SaveAsync(cart);
    }

    public async Task RemoveProductFromCartAsync(Guid cartId, Guid productId)
    {
        // Fetch the cart from OrderDatabase
        var cart = await _cartRepository.GetByIdAsync(cartId);
        if (cart == null)
        {
            throw new Exception("Cart not found.");
        }

        // Find and remove the product from the cart
        var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.CartItems.Remove(item);

            // Release the product reservation
            await _bulkProductUpdateService.ReleaseProductReservationAsync(new List<Guid> { productId });

            // Update the expiration time (15 minutes)
            cart.ExpirationTime = DateTime.UtcNow.AddMinutes(15);
            cart.LastUpdated = DateTime.UtcNow;

            // Save the updated cart in OrderDatabase
            await _cartRepository.SaveAsync(cart);
        }
    }
}