using OrderProvider.Core.DTOs;
using OrderProvider.Core.Interfaces.Services;
using OrderProvider.Messaging.AzureServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProvider.Core.Services
{
    public class BulkProductUpdateService : IBulkProductUpdateService
    {
        private readonly IAzureServiceBusPublisher _azureServiceBusPublisher;

        public BulkProductUpdateService(IAzureServiceBusPublisher azureServiceBusPublisher)
        {
            _azureServiceBusPublisher = azureServiceBusPublisher;
        }

        public async Task UpdateReservedUntilAsync(List<Guid> productIds, DateTime reservedUntil)
        {
            var productUpdates = productIds.ConvertAll(productId => new BulkProductsUpdateDto
            {
                ProductId = productId,
                ReservedUntil = reservedUntil,
                ActionType = "Reserve",
                Status = "Reserved",
                UpdatedAt = DateTime.UtcNow
            });

            await PublishBulkUpdateToQueueAsync(productUpdates);
        }

        public async Task ReserveProductsAsync(List<Guid> productIds)
        {
            var productUpdates = productIds.ConvertAll(productId => new BulkProductsUpdateDto
            {
                ProductId = productId,
                ReservedUntil = DateTime.UtcNow.AddMinutes(15),
                ActionType = "Reserve",
                Status = "Reserved",
                UpdatedAt = DateTime.UtcNow
            });

            await PublishBulkUpdateToQueueAsync(productUpdates);
        }

        public async Task MarkProductsAsSoldAsync(List<Guid> productIds, DateTime soldUntil)
        {
            var productUpdates = productIds.ConvertAll(productId => new BulkProductsUpdateDto
            {
                ProductId = productId,
                SoldUntil = soldUntil,
                ActionType = "Sell",
                Status = "Sold",
                UpdatedAt = DateTime.UtcNow
            });

            await PublishBulkUpdateToQueueAsync(productUpdates);
        }

        public async Task ProcessBulkProductUpdateAsync(List<BulkProductsUpdateDto> productUpdates)
        {
            foreach (var update in productUpdates)
            {
                update.UpdatedAt = DateTime.UtcNow; // Ensure timestamp is updated
            }

            await PublishBulkUpdateToQueueAsync(productUpdates);
        }

        public async Task PublishBulkUpdateToQueueAsync(List<BulkProductsUpdateDto> productUpdates)
        {
            await _azureServiceBusPublisher.PublishAsync(productUpdates);
        }

        // Implementing the new method to release product reservation
        public async Task ReleaseProductReservationAsync(List<Guid> productIds)
        {
            var productUpdates = productIds.ConvertAll(productId => new BulkProductsUpdateDto
            {
                ProductId = productId,
                ReservedUntil = null,  // Setting ReservedUntil to null to release reservation
                ActionType = "Release",
                Status = "Available",
                UpdatedAt = DateTime.UtcNow
            });

            await PublishBulkUpdateToQueueAsync(productUpdates);
        }
    }
}
