using OrderProvider.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProvider.Core.Interfaces.Services
{
    public interface IBulkProductUpdateService
    {
        Task UpdateReservedUntilAsync(List<Guid> productIds, DateTime reservedUntil);
        Task ReserveProductsAsync(List<Guid> productIds);
        Task MarkProductsAsSoldAsync(List<Guid> productIds, DateTime soldUntil);
        Task ProcessBulkProductUpdateAsync(List<BulkProductsUpdateDto> productUpdates);
        Task PublishBulkUpdateToQueueAsync(List<BulkProductsUpdateDto> productUpdates);
        Task ReleaseProductReservationAsync(List<Guid> productIds);  // Added method
    }
}
