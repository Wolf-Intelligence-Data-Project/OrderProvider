namespace OrderProvider.Core.Models
{
    public class CreatePaymentSessionRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }  // Total amount of the order
    }

}
