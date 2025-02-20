using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<ProductEntity> GetProductByIdAsync(Guid productId);
        Task<List<ProductEntity>> GetReservedProductsByUserIdAsync(Guid userId);
        Task BulkUpdateProductsAsync(List<ProductEntity> products);
    }
}
