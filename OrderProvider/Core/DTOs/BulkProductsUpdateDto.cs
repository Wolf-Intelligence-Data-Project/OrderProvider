using System;

namespace OrderProvider.Core.DTOs
{
    public class BulkProductsUpdateDto
    {
        public Guid ProductId { get; set; }  // Changed from int to Guid
        public DateTime? ReservedUntil { get; set; }
        public DateTime? SoldUntil { get; set; }
        public string ActionType { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
