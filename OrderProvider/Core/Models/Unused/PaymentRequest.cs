namespace OrderProvider.Core.Models.NewFolder
{
    public class PaymentRequest
    {
        public Guid OrderId { get; set; }  // The order being paid for
        public string PaymentMethod { get; set; }  // E.g., "Credit Card", "PayPal", etc.
        public decimal Amount { get; set; }  // The total amount to be paid
        public DateTime PaymentDate { get; set; }  // Timestamp when the payment is made
        public string PaymentStatus { get; set; }  // E.g., "Pending", "Completed"
        public string PaymentTransactionId { get; set; }  // Unique ID for the payment transaction
    }

}
