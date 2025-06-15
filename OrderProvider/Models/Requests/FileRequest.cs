using System.ComponentModel.DataAnnotations;

namespace OrderProvider.Models.Requests;


public class FileRequest
{
    [Required]
    public string OrderId { get; set; }
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public DateTime SoldUntil { get; set; }

    public string FullName { get; set; }

    public bool IsCompany { get; set; }

    public string CompanyName { get; set; }

    public string CEO { get; set; }

    [Required]
    public string Email { get; set; }

}
