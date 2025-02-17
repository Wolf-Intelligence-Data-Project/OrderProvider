namespace OrderProvider.Core.Entities
{
    public class CartEntity
    {
        public Guid Id { get; set; } // Unique Cart ID
        public string UserId { get; set; } // Link to the user who owns the cart
        public DateTime CreatedAt { get; set; } // When the cart was created
        public DateTime LastUpdated { get; set; } // Last time the cart was updated
        public ICollection<CartItemEntity> CartItems { get; set; } // List of products in the cart
        public DateTime? ExpirationTime { get; set; }  // Expiration time for 15 minutes
    }
}
