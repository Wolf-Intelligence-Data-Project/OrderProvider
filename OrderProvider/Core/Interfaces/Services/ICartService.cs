using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderProvider.Core.Models;
namespace OrderProvider.Core.Interfaces.Services
{
    public interface ICartService
    {
        Task AddProductToCartAsync(Guid cartId, AddToCartRequest request);
        Task UpdateCartAsync(Guid cartId, UpdateCartRequest request);
        Task RemoveProductFromCartAsync(Guid cartId, Guid productId);
    }
}
