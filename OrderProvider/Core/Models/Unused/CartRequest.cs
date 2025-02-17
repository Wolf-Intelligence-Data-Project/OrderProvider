namespace OrderProvider.Core.Models.NewFolder
{
    public class CartRequest
    {
        public List<Guid> ProductIds { get; set; }  // List of unique product IDs
        public string EmailAddress { get; set; }  // Email to which the digital product will be delivered
        public DateTime AddedToCart { get; set; }  // Timestamp for when the product is added to the cart
    }

}
