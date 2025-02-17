namespace OrderProvider.Core.Entities;

public class Order
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }

    // Link to the cart
    public Guid CartId { get; set; }
    public CartEntity Cart { get; set; }

    // Add CartItems directly here if needed
    public ICollection<CartItemEntity> CartItems { get; set; }  // New line
}