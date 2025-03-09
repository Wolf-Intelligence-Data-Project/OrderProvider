using OrderProvider.Interfaces.Services;

namespace OrderProvider.Services.ServiceBus;

public class PaymentConfirmationListener
{
    private readonly RabbitMQService _rabbitMQService;
    private readonly ILogger<PaymentConfirmationListener> _logger;
    private readonly IOrderService _orderService;

    public PaymentConfirmationListener(RabbitMQService rabbitMQService, ILogger<PaymentConfirmationListener> logger, IOrderService orderService)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
        _orderService = orderService;
    }

    // Listen for payment confirmation messages
    public async Task ListenForPaymentConfirmationAsync()
    {
        try
        {
            var paymentConfirmedMessage = await _rabbitMQService.ReceiveMessageAsync("payment-confirmed");

            if (string.IsNullOrEmpty(paymentConfirmedMessage))
            {
                _logger.LogWarning("No payment confirmation received.");
                return;
            }

            _logger.LogInformation("Payment confirmed for OrderId: {OrderId}", paymentConfirmedMessage);

            // Here we can send to FileProvider and InvoiceProvider after payment confirmation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment confirmation.");
        }
    }
}
