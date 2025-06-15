using Newtonsoft.Json;

namespace OrderProvider.Models.Responses;

public class KlarnaPaymentResponse
{
    [JsonProperty("order_id")]
    public string OrderId { get; set; }

    [JsonProperty("session_id")]
    public string SessionId { get; set; }

    [JsonProperty("redirect_url")]
    public string RedirectUrl { get; set; }
}
