using System;

namespace OrderProvider.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
    }
}
