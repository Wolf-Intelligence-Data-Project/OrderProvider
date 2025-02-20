namespace OrderProvider.ServiceBus
{
    public class FileEvent
    {
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
    }
}
