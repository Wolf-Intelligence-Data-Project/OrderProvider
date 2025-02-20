namespace OrderProvider.Interfaces.Services
{
    public interface IInvoiceProvider
    {
        Task GenerateInvoiceAsync(Guid orderId);
    }

}