namespace OrderProvider.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

}
