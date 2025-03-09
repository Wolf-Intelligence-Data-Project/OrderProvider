namespace OrderProvider.Models.DTOs;

public class SoldDto
{
    public Guid CustomerId { get; set; }
    public DateTime? SoldUntil { get; set; }
}
