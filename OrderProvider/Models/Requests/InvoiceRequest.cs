using System;

namespace OrderProvider.Models.Requests
{
    public class InvoiceRequest
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FiltersUsed { get; set; }  // Added FiltersUsed
        public decimal PricePerProductAtPurchase { get; set; }  // Added Price Per Product
        public int Quantity { get; set; }  // Added Quantity
    }
}
