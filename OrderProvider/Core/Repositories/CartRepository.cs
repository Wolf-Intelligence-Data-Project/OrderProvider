using Microsoft.EntityFrameworkCore;
using OrderProvider.Core.Entities;
using OrderProvider.Core.Interfaces.Repositories;
using OrderProvider.Persistence.Data;

namespace OrderProvider.Core.Repositories;

public class CartRepository : ICartRepository
{
    private readonly OrderDbContext _orderDbContext;

    public CartRepository(OrderDbContext context)
    {
        _orderDbContext = context;
    }

    // Get the cart by user ID
    public async Task<CartEntity> GetCartByUserIdAsync(string userId)
    {
        return await _orderDbContext.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    // Add a product to the cart (unique item, 1 quantity per item)
    public async Task AddProductToCartAsync(string userId, Guid productId)
    {
        var cart = await _orderDbContext.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new CartEntity
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                CartItems = new List<CartItemEntity>()
            };
            _orderDbContext.Carts.Add(cart);
            await _orderDbContext.SaveChangesAsync();
        }

        // Ensure product is unique in the cart
        if (!cart.CartItems.Any(i => i.ProductId == productId))
        {
            cart.CartItems.Add(new CartItemEntity
            {
                ProductId = productId,
                ReservedUntil = DateTime.UtcNow.AddMinutes(15)  // Reserve product for 15 minutes
            });
            cart.LastUpdated = DateTime.UtcNow;
            await _orderDbContext.SaveChangesAsync();
        }
    }

    // Update the cart by adding/removing products
    public async Task UpdateCartAsync(string userId, List<Guid> productIds)
    {
        var cart = await _orderDbContext.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return;

        // Remove products that are no longer in the cart
        var itemsToRemove = cart.CartItems.Where(i => !productIds.Contains(i.ProductId)).ToList();
        foreach (var item in itemsToRemove)
        {
            cart.CartItems.Remove(item);
        }

        // Add new products to the cart
        foreach (var productId in productIds)
        {
            if (!cart.CartItems.Any(i => i.ProductId == productId))
            {
                cart.CartItems.Add(new CartItemEntity
                {
                    ProductId = productId,
                    ReservedUntil = DateTime.UtcNow.AddMinutes(15)  // Reserve product for 15 minutes
                });
            }
        }

        cart.LastUpdated = DateTime.UtcNow;
        await _orderDbContext.SaveChangesAsync();
    }

    // Remove a product from the cart
    public async Task RemoveProductFromCartAsync(string userId, Guid productId)
    {
        var cart = await _orderDbContext.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return;

        var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.CartItems.Remove(item);
            cart.LastUpdated = DateTime.UtcNow;
            await _orderDbContext.SaveChangesAsync();
        }
    }

    // Get cart by its ID
    public async Task<CartEntity> GetByIdAsync(Guid cartId)
    {
        return await _orderDbContext.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }

    // Save the cart (useful for updates)
    public async Task SaveAsync(CartEntity cart)
    {
        _orderDbContext.Carts.Update(cart);
        await _orderDbContext.SaveChangesAsync();
    }

    // Bulk update of products reserved until the cart expires
    public async Task BulkUpdateReservedUntilAsync(IEnumerable<Guid> productIds, DateTime reservedUntil)
    {
        var cartItemsToUpdate = await _orderDbContext.CartItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync();

        foreach (var cartItem in cartItemsToUpdate)
        {
            cartItem.ReservedUntil = reservedUntil;
        }

        await _orderDbContext.SaveChangesAsync();
    }
}
