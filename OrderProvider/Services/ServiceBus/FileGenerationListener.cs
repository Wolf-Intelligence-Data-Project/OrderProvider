using Azure;
using Newtonsoft.Json;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Requests;
using OrderProvider.Models.Responses.ServiceBus;

namespace OrderProvider.Services.ServiceBus;

public class FileGenerationListener
{
    private readonly RabbitMQService _rabbitMQService;
    private readonly ILogger<FileGenerationListener> _logger;
    private readonly IOrderService _orderService;

    public FileGenerationListener(RabbitMQService rabbitMQService, ILogger<FileGenerationListener> logger, IOrderService orderService)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
        _orderService = orderService;
    }
    public async Task ListenForFileGenerationAsync()
    {
        // Listen for file generation completion
        var message = await _rabbitMQService.ReceiveMessageAsync("file-generation-completed");

        if (string.IsNullOrEmpty(message))
        {
            _logger.LogWarning("No message received for file generation.");
            return;
        }

        _logger.LogInformation("Received file generation completion message.");

        try
        {
            // Deserialize the message to check if the process was successful
            var response = JsonConvert.DeserializeObject<FileGenerationResponse>(message);

            // Convert string OrderId and CustomerId to Guid
            if (Guid.TryParse(response.OrderId, out var orderId) && Guid.TryParse(response.CustomerId, out var customerId))
            {
                if (response == null || !response.IsSuccess)
                {
                    // File generation failed, revert the order
                    await _orderService.RevertOrderAsync(customerId, orderId);
                    _logger.LogError("File generation failed, order reverted.");
                }
                else
                {
                    _logger.LogInformation("File generation succeeded.");
                }
            }
            else
            {
                _logger.LogError("Invalid OrderId or CustomerId format. OrderId: {OrderId}, CustomerId: {CustomerId}", response.OrderId, response.CustomerId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file generation message for revokation initiation.");
        }
    }
}