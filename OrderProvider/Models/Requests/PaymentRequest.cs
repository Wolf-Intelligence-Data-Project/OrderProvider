using System.ComponentModel.DataAnnotations;

namespace OrderProvider.Models.Requests;

public class PaymentRequest
{
    [Required]
    public string CardNumber { get; set; }
    [Required]
    public string CardExpiration { get; set; }
    [Required]
    public string CVV { get; set; }

}
