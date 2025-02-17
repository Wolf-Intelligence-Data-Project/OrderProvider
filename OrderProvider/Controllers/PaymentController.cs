using Microsoft.AspNetCore.Mvc;
using OrderProvider.Core.Interfaces.Services;
using OrderProvider.Core.Models;
namespace OrderProvider.Controllers;
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("create-payment-session")]
    public async Task<IActionResult> CreatePaymentSession([FromBody] CreatePaymentSessionRequest request)
    {
        var sessionId = await _paymentService.CreateKlarnaPaymentSessionAsync(request.OrderId, request.Amount);

        // Return the Klarna session ID to redirect the user to Klarna's payment page
        return Ok(new { sessionId });
    }

    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        bool isPaid = await _paymentService.ConfirmKlarnaPaymentAsync(request.OrderId);

        if (isPaid)
        {
            return Ok("Payment confirmed successfully");
        }
        else
        {
            return BadRequest("Payment confirmation failed");
        }
    }
}
