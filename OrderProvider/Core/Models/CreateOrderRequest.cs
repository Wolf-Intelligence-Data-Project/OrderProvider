namespace OrderProvider.Core.DTOs
{
    public class CreateOrderRequest
    {
        public string UserId { get; set; } // Fetch cart based on this
        public int CompanyId { get; set; }
    }

}
