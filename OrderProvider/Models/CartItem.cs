namespace OrderProvider.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime ReservedUntil { get; set; }
    }
}
