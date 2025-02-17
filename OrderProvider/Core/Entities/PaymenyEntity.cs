namespace OrderProvider.Core.Entities
{
    public class PaymentEntity
    {
        public int Id { get; set; } // Unique identifier for the payment
        public int OrderId { get; set; } // Reference to the associated order
        public DateTime PaymentDate { get; set; } // Date when the payment was made
        public decimal Amount { get; set; } // Amount paid
        public string PaymentStatus { get; set; } // Status of the payment (e.g., "Pending", "Completed", "Failed")
        public string TransactionId { get; set; } // Transaction ID from the payment API
    }

}
