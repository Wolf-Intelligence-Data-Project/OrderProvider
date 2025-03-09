using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrderProvider.Models.Requests;

public class ProductReserveRequest
{
    [JsonPropertyName("companyId")]
    [Required]
    public Guid CompanyId { get; set; }

    [JsonPropertyName("businessTypes")]
    public List<string>? BusinessTypes { get; set; }

    [JsonPropertyName("regions")]
    public List<string>? Regions { get; set; }

    [JsonPropertyName("cities")]
    public List<string>? Cities { get; set; }

    [JsonPropertyName("citiesByRegion")]
    public List<string>? CitiesByRegion { get; set; }

    [JsonPropertyName("postalCodes")]
    public List<string>? PostalCodes { get; set; }

    [JsonPropertyName("minRevenue")]
    public int? MinRevenue { get; set; }

    [JsonPropertyName("maxRevenue")]
    public int? MaxRevenue { get; set; }

    [JsonPropertyName("minNumberOfEmployees")]
    public int? MinNumberOfEmployees { get; set; }

    [JsonPropertyName("maxNumberOfEmployees")]
    public int? MaxNumberOfEmployees { get; set; }

    [JsonPropertyName("quantityOfFiltered")]

    [Required]
    public int QuantityOfFiltered { get; set; }
}
