namespace OrderProvider.Models.Responses;
public class ProductFilterResponse
{
    public int AvailableQuantity { get; set; }
    public decimal TotalPriceBeforeVat { get; set; }
    public decimal TotalPrice { get; set; }
}
