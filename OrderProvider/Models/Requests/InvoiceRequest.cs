using System;

namespace OrderProvider.Models.Requests
{
    public class InvoiceRequest
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FiltersUsed { get; set; }
        public decimal PricePerProductAtPurchase { get; set; }
        public int Quantity { get; set; }
    }
}
