namespace OrderProvider.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal PricePerProductAtPurchase { get; set; }  // Price per product at the time of order
        public int Quantity { get; set; }  // Number of unique products in the order
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string FiltersUsed { get; set; }  // JSON or string describing filters applied
    }
}
