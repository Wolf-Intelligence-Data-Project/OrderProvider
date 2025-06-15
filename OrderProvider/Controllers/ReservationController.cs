using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.Requests;

namespace OrderProvider.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(ILogger<ReservationController> logger, IReservationService reservationService)
    {
        _logger = logger;
        _reservationService = reservationService;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveProducts([FromBody] ProductReserveRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model state is invalid: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(new { message = "Ogiltig begäran.", errors = ModelState });
        }

        if (request?.QuantityOfFiltered == null)
        {
            _logger.LogWarning("QuantityOfFiltered is required but was null.");
            return BadRequest(new { message = "Antalet måste anges (QuantityOfFiltered)." });
        }

        _logger.LogInformation("Raw request body: {RawRequest}", JsonConvert.SerializeObject(request));

        _logger.LogInformation("Requested quantity: {Quantity}", request.QuantityOfFiltered);

        if (request.BusinessTypes == null)
        {
            _logger.LogInformation("No BusinessTypes provided.");
            request.BusinessTypes = new List<string>();
        }

        if (request.Cities == null)
        {
            _logger.LogInformation("No Cities provided.");
            request.Cities = new List<string>();
        }

        try
        {
            var reservation = await _reservationService.ReserveProductsAsync(request);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation failed or returned null.");
                return BadRequest(new { message = "Reservation failed." });
            }

            _logger.LogInformation("Reservation successful: {@Reservation}", reservation);

            return Ok(new
            {
                message = "Produkter har reserverats",
                reservation.Quantity
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access during reservation.");
            return Unauthorized(new { message = "Behörighet saknas." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during reservation.");
            return StatusCode(500, new { message = "Ett internt fel uppstod vid reservation." });
        }
    }

    [HttpPost("get-reservation")]
    public async Task<IActionResult> GetReservation()
    {

        try
        {
            var reservation = await _reservationService.GetReservationByUserIdAsync();

            if (reservation == null)
            {
                _logger.LogWarning("No reservation found.");
                return NotFound(new { message = "Tomt val." });
            }

            _logger.LogInformation("Reservation fetched successfully");
            return Ok(new { reservation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting the reservation.");
            return StatusCode(500, new { message = "An error occurred while fetching reservation." });
        }
    }



    [HttpDelete("delete-reservation")]
    public async Task<IActionResult> DeleteReservation()
    {
        var isDeleted = await _reservationService.DeleteReservationNow();

        if (!isDeleted)
        {
            _logger.LogWarning("No reservation found to delete.");
            return NotFound(new { message = "Reservation not found or could not be deleted." });
        }

        return Ok(new { message = "Reservation deleted successfully." });
    }
}
