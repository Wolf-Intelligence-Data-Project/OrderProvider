using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrderProvider.Models.Requests;

public class ProductReserveRequest
{
    public List<string> BusinessTypes { get; set; }
    public List<string> Cities { get; set; }
    public List<string> CitiesByRegion { get; set; }
    public List<string> PostalCodes { get; set; }
    public int? MinRevenue { get; set; }
    public int? MaxRevenue { get; set; }
    public int? MinNumberOfEmployees { get; set; }
    public int? MaxNumberOfEmployees { get; set; }
    public List<string> Regions { get; set; }

    [Required]
    public int QuantityOfFiltered { get; set; }
}
