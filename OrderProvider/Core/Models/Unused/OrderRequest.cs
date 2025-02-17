namespace OrderProvider.Core.Models.NewFolder
{
    public class OrderRequest
    {
        public string CustomerId { get; set; }  // Represents the company or individual
        public string EmailAddress { get; set; }  // Email where the purchased product will be delivered
        public List<OrderItemRequest> OrderItems { get; set; }
        public decimal TotalAmount { get; set; }  // The total cost of the order
        public DateTime OrderDate { get; set; }  // Date the order is placed
    }
}
