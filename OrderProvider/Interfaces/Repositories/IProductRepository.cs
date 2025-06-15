using OrderProvider.Data;
using OrderProvider.Entities;
using OrderProvider.Models.Requests;

namespace OrderProvider.Interfaces.Repositories;

public interface IProductRepository
{
    Task<ReservationEntity> GetReservationByIdAsync(Guid reservationId);
    Task<List<ProductEntity>> GetReservedProductsByUserIdAsync(Guid userId);
    Task BulkUpdateProductsAsync(List<ProductEntity> products);
    Task ProductSoldAsync(Guid userId);
    Task<List<Guid>> GetProductIdsForReservationAsync(ProductReserveRequest filters, List<string> rawBusinessTypes);

    Task ReserveProductsByIdsAsync(List<Guid> productIds, Guid userId);
    Task DeleteExpiredReservationsAsync(ProductDbContext context, DateTime timeCheckUntil, DateTime timeCheckFrom);
    Task RemoveReservationsAsync(Guid userId);

    Task<IEnumerable<ProductEntity>> GetProductsByCustomerIdAsync(Guid customerId);
    Task UpdateProductAsync(ProductEntity product);
}
