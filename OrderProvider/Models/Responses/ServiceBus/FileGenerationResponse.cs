using System.ComponentModel.DataAnnotations;

namespace OrderProvider.Models.Responses.ServiceBus;

public class FileGenerationResponse
{
    [Required]
    public string OrderId { get; set; } = null!;
    [Required]
    public string CustomerId { get; set; } = null!;
    [Required]
    public bool IsSuccess { get; set; }
}
