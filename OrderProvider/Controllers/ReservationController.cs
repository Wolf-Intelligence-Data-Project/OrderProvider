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
        // Log the raw incoming request (request body) before any processing or conversion
        _logger.LogInformation("Raw request body: {RawRequest}", JsonConvert.SerializeObject(request));

        if (request == null)
        {
            _logger.LogWarning("Request body is null.");
            return BadRequest(new { message = "Invalid request." });  // Return a JSON response
        }

        if (request.QuantityOfFiltered == null)
        {
            _logger.LogWarning("QuantityOfFiltered is null.");
        }
        else if (request.QuantityOfFiltered == 0)
        {
            _logger.LogWarning("QuantityOfFiltered is 0.");
        }
        else
        {
            _logger.LogInformation("Valid QuantityOfFiltered: {Quantity}", request.QuantityOfFiltered);
        }

        // Log the entire request object after any null checks or required modifications
        _logger.LogInformation("Received reservation request: {Request}", JsonConvert.SerializeObject(request));

        // Check and handle null or empty fields
        if (request.BusinessTypes == null || request.BusinessTypes.Count == 0)
        {
            _logger.LogInformation("No BusinessTypes provided, defaulting to empty array.");
            request.BusinessTypes = new List<string>(); // or handle as needed
        }

        if (request.Cities == null || request.Cities.Count == 0)
        {
            _logger.LogInformation("No Cities provided, defaulting to empty array.");
            request.Cities = new List<string>(); // or handle as needed
        }

        // Process reservation and get the full ReservationDto
        var reservation = await _reservationService.ReserveProductsAsync(request);

        if (reservation == null)
        {
            _logger.LogWarning("Reservation failed or returned null.");
            return BadRequest(new { message = "Reservation failed." });
        }

        // Log reserved details
        _logger.LogInformation("Reservation successful: {@Reservation}", reservation);

        // Return a JSON response with the full reservation details
        return Ok(new
        {
            message = "Products reserved successfully",
            reservation.Quantity
        });
    }

    [HttpPost("get-reservation")]
    public async Task<IActionResult> GetReservation([FromBody] GetReservationRequest request)
    {
        // Log the entire request to see what is coming in
        _logger.LogInformation($"Received request: {JsonConvert.SerializeObject(request)}");

        if (string.IsNullOrEmpty(request.UserId))
        {
            return BadRequest(new { message = "userId is required." });
        }

        if (Guid.TryParse(request.UserId, out var parsedGuid))
        {
            var reservation = await _reservationService.GetReservationByUserIdAsync(parsedGuid);

            if (reservation == null)
            {
                _logger.LogWarning("No reservation found for user ID: {UserId}", request.UserId);
                return NotFound(new { message = "No reservation found." });
            }

            return Ok(new { reservation });
        }

        _logger.LogError($"Invalid userId format: {request.UserId}");
        return BadRequest(new { message = "Invalid user ID format." });
    }

    [HttpDelete("delete-reservation")]
    public async Task<IActionResult> DeleteReservation(Guid userId)
    {
        var isDeleted = await _reservationService.DeleteReservationNow(userId);

        if (!isDeleted)
        {
            _logger.LogWarning("No reservation found to delete for user ID: {UserId}", userId);
            return NotFound(new { message = "Reservation not found or could not be deleted." });
        }

        return Ok(new { message = "Reservation deleted successfully." });
    }
}
