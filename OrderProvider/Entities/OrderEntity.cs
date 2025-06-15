using System.ComponentModel.DataAnnotations;

namespace OrderProvider.Entities;

public class OrderEntity
{
    [Key]
    public Guid OrderId { get; set; }
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public string CustomerEmail { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm"));
    [Required]
    public decimal PricePerProduct { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public decimal TotalPriceWithoutVat { get; set; }
    [Required]
    public decimal TotalPrice { get; set; }
    [Required]
    public string PaymentStatus { get; set; } = "Pending";

    public Guid FiltersUsed { get; set; }

    public string? KlarnaPaymentId { get; set; }
}
