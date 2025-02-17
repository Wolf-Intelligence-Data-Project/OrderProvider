namespace OrderProvider.Core.Entities
{
    public class CartItemEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public DateTime? ReservedUntil { get; set; }
        public Guid CartId { get; set; } // Foreign key to CartEntity
        public CartEntity Cart { get; set; } // Navigation property
    }
}
