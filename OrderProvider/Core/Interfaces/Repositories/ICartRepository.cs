using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderProvider.Core.Entities;

namespace OrderProvider.Core.Interfaces.Repositories
{
    public interface ICartRepository
    {
        Task<CartEntity> GetByIdAsync(Guid cartId);
        Task SaveAsync(CartEntity cart);
        Task<CartEntity> GetCartByUserIdAsync(string userId);
        Task AddProductToCartAsync(string userId, Guid productId);
        Task UpdateCartAsync(string userId, List<Guid> productIds);
        Task RemoveProductFromCartAsync(string userId, Guid productId);
    }
}
