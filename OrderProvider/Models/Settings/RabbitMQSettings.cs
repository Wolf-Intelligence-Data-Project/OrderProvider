namespace OrderProvider.Models.Settings;

public class RabbitMQSettings
{
    public string HostName { get; set; }
    public string QueueNameOrderCreated { get; set; }
    public string QueueNamePaymentConfirmed { get; set; }
    public string QueueNameFileGeneration { get; set; }
    public string QueueNameInvoiceGeneration { get; set; }
}