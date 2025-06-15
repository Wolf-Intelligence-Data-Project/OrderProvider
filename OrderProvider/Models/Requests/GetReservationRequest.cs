using System.ComponentModel.DataAnnotations;

namespace OrderProvider.Models.Requests;

public class GetReservationRequest
{
    [Required]
    public string UserId { get; set; } 
}