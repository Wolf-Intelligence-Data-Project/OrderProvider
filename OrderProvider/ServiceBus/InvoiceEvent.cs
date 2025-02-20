namespace OrderProvider.ServiceBus
{
    public class InvoiceEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
