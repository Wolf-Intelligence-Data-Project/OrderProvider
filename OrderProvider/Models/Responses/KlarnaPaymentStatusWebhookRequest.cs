namespace OrderProvider.Models.Responses;

public class KlarnaPaymentStatusWebhookRequest
{
    public string OrderId { get; set; }  // The order ID from Klarna
    public string Status { get; set; }   // The payment status (e.g., "paid", "pending", etc.)
    public string TransactionId { get; set; }  // The payment transaction ID
                                               // You can include other fields depending on Klarna's webhook response
}
