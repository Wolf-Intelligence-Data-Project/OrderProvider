using Newtonsoft.Json;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Responses.ServiceBus;

namespace OrderProvider.Services.ServiceBus;

public class InvoiceGenerationListener
{
    private readonly RabbitMQService _rabbitMQService;
    private readonly ILogger<InvoiceGenerationListener> _logger;
    private readonly IOrderService _orderService;

    public InvoiceGenerationListener(RabbitMQService rabbitMQService, ILogger<InvoiceGenerationListener> logger, IOrderService orderService)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
        _orderService = orderService;
    }

    public async Task ListenForInvoiceGenerationAsync()
    {
        // Listen for invoice generation completion
        var message = await _rabbitMQService.ReceiveMessageAsync("invoice-generation-completed");

        if (string.IsNullOrEmpty(message))
        {
            _logger.LogWarning("No message received for invoice generation.");
            return;
        }

        _logger.LogInformation("Received invoice generation completion message.");

        try
        {
            // Deserialize the message to check if the process was successful
            var response = JsonConvert.DeserializeObject<InvoiceGenerationResponse>(message);
            if (Guid.TryParse(response.OrderId, out var orderId) && Guid.TryParse(response.CustomerId, out var customerId))
            {
                if (response == null || !response.IsSuccess)
                {
                    // Invoice generation failed, revert the order
                    await _orderService.RevertOrderAsync(customerId, orderId);
                    _logger.LogError("Invoice generation failed, order reverted.");
                }
                else
                {
                    _logger.LogInformation("Invoice generation succeeded.");
                }
            }
                else
                {
                    _logger.LogError("Invalid OrderId or CustomerId format. OrderId: {OrderId}, CustomerId: {CustomerId}", response.OrderId, response.CustomerId);
                }
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice generation message for revokation initiation.");
        }
    }
}
