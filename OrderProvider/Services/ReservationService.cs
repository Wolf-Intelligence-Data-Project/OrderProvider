using Microsoft.Extensions.Options;
using OrderProvider.Data;
using OrderProvider.Factories;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.Models.DTOs;
using OrderProvider.Models.Enums;
using OrderProvider.Models.Requests;
using OrderProvider.Models.Settings;
using System.ComponentModel;

namespace OrderProvider.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IProductRepository _productRepository;
    private System.Timers.Timer _timer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReservationService> _logger;
    private readonly IOptions<PriceSettings> _priceSettings;

    public ReservationService(IReservationRepository reservationRepository, IProductRepository productRepository, IServiceScopeFactory serviceScopeFactory, IOptions<PriceSettings> priceSettings, ILogger<ReservationService> logger)
    {
        _reservationRepository = reservationRepository;
        _productRepository = productRepository;
        _serviceScopeFactory = serviceScopeFactory;
        _priceSettings = priceSettings;
        _logger = logger;
    }
    public async Task<ReservationDto> ReserveProductsAsync(ProductReserveRequest request)
    {
        var companyId = request.CompanyId;
        var quantity = request.QuantityOfFiltered;

        _logger.LogInformation("ReserveProductsAsync started for CompanyId: {CompanyId} with Quantity: {Quantity}", companyId, quantity);

        if (quantity > 0 && companyId != Guid.Empty)
        {
            try
            {
                _logger.LogInformation("Deleting existing reservations for CompanyId: {CompanyId}", companyId);
                await DeleteReservationNow(companyId);

                _logger.LogInformation("Fetching product IDs for reservation...");

                // Get both raw and formatted business types
                var (rawBusinessTypes, formattedBusinessTypes) = FormatBusinessTypes(request.BusinessTypes);

                // Log the formatted business types for visibility
                _logger.LogInformation("Formatted business types: {BusinessTypes}", string.Join(", ", formattedBusinessTypes));

                // Save the formatted business types in the request object for reservation
                request.BusinessTypes = formattedBusinessTypes;

                // Pass only raw business types for querying the database
                var productIds = await _productRepository.GetProductIdsForReservationAsync(request, rawBusinessTypes);
                if (productIds == null || !productIds.Any())
                {
                    _logger.LogWarning("No products found for reservation.");
                }

                // Log the number of products found
                _logger.LogInformation("Found {ProductCount} products for reservation.", productIds?.Count());

                // Reserve products by their IDs
                _logger.LogInformation("Reserving {ProductCount} products for CompanyId: {CompanyId}", productIds.Count(), companyId);
                await _productRepository.ReserveProductsByIdsAsync(productIds, companyId);

                // Create and save reservation with the formatted business types
                var reservation = ReservationFactory.CreateReservationEntity(request);
                _logger.LogInformation("Created reservation with ID: {ReservationId}", reservation);

                await _reservationRepository.AddReservationAsync(reservation);
                _logger.LogInformation("Reservation saved to database with ID: {ReservationId}", reservation);

                // Start a timer to release expired reservations
                StartReservationReleaseTimer();
                _logger.LogInformation("Reservation release timer started.");

                return ReservationFactory.CreateReservationDto(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during reservation process for CompanyId: {CompanyId}: {ErrorMessage}", companyId, ex.Message);
                _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
                return null!;
            }
        }

        _logger.LogWarning("Reservation not processed. Invalid quantity or company ID. CompanyId: {CompanyId}, Quantity: {Quantity}", companyId, quantity);
        return null!;
    }

    private (List<string> rawBusinessTypes, List<string> formattedBusinessTypes) FormatBusinessTypes(List<string>? businessTypes)
    {
        if (businessTypes == null || !businessTypes.Any())
            return (new List<string>(), new List<string>());

        // Create formatted business types for saving in the database
        var formattedBusinessTypes = businessTypes.Select(code =>
        {
            var trimmedCode = code.Trim().Split('.')[0];  // Get everything before the dot

            if (Enum.TryParse(trimmedCode, out BusinessType businessType))
            {
                var description = businessType.GetType()
                                              .GetField(businessType.ToString())
                                              .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                              .FirstOrDefault() as DescriptionAttribute;
                return description != null ? $"{description.Description} ({trimmedCode})" : trimmedCode;
            }
            return trimmedCode;  // Return only the part before the dot
        }).ToList();

        // Extract raw business types (before the dot) for querying, and add space after the first letter
        var rawBusinessTypes = businessTypes.Select(code =>
        {
            var trimmedCode = code.Trim().Split('.')[0];  // Get everything before the dot

            // Add space after the first letter if there's a number following it
            if (trimmedCode.Length > 1 && char.IsLetter(trimmedCode[0]) && char.IsDigit(trimmedCode[1]))
            {
                trimmedCode = trimmedCode.Insert(1, " ");  // Insert space after the first letter
            }

            return trimmedCode;
        }).ToList();

        // Return both raw codes for querying and formatted codes for saving
        return (rawBusinessTypes, formattedBusinessTypes);
    }


    private (decimal priceWithoutVat, decimal totalPrice) CalculatePrice(int count)
    {
        // Get price and VAT rate from configuration
        decimal pricePerProduct = _priceSettings.Value.PricePerProduct; // SEK per product
        decimal vatRate = _priceSettings.Value.VatRate; // VAT rate (e.g., 25 for 25%)

        // Ensure price settings are being loaded
        if (pricePerProduct <= 0 || vatRate <= 0)
        {
            _logger.LogError("Invalid price settings: PricePerProduct = {PricePerProduct}, VatRate = {VatRate}",
                pricePerProduct, vatRate);
            throw new InvalidOperationException("Invalid price settings.");
        }

        // Calculate the total price before VAT
        decimal baseTotalPrice = count * pricePerProduct;

        // Calculate the total price after VAT
        decimal totalPriceWithVat = baseTotalPrice * (1 + vatRate / 100); // Apply VAT to total price

        return (baseTotalPrice, totalPriceWithVat);
    }

    public async Task<ReservationDto> GetReservationByUserIdAsync(Guid companyId)
    {

        var reservation = await _reservationRepository.GetReservationByUserIdAsync(companyId);

        if (reservation == null)
        {
            return null;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            // First delete expired reservations if there are some (This one secures that reservation fetch is fresh and accurate, even if orderprovider was down and reservations did not delete automaticaly)
            var (timeCheckFrom, timeCheckUntil) = GetReservationTimeWindow();

            await _productRepository.DeleteExpiredReservationsAsync(context, timeCheckUntil, timeCheckFrom);

            // Calculate prices based on reservation quantity
            var (priceWithoutVat, totalPriceWithVat) = CalculatePrice(reservation.Quantity);

            // Create ReservationDto and include calculated prices
            var reservationDto = ReservationFactory.CreateReservationDto(reservation);

            // Add the calculated prices to the ReservationDto
            reservationDto.PriceWithoutVat = priceWithoutVat;
            reservationDto.TotalPrice = totalPriceWithVat;

            return reservationDto;

        }
    }

    private async Task<bool> DeleteReservationByUserIdAsync(Guid companyId)
    {
        var reservation = await _reservationRepository.GetReservationByUserIdAsync(companyId);
        if (reservation != null)
        {
            await _reservationRepository.DeleteReservationImmediatelyAsync(reservation.ReservationId);
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteReservationNow(Guid companyId)
    {
        try
        {
            await _productRepository.RemoveReservationsAsync(companyId);
            await DeleteReservationByUserIdAsync(companyId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void StartReservationReleaseTimer()
    {
        _timer?.Dispose();  // Dispose old timer if exists
        _timer = new System.Timers.Timer(903000); // 15 minutes and 3 seconds
        _timer.Elapsed += async (sender, e) => await TimerElapsedAsync();
        _timer.AutoReset = false; // Ensure it runs only once
        _timer.Start();
        _logger.LogInformation("TIMER STARTED for for reservations cleanup");
    }

    private async Task TimerElapsedAsync()
    {
        try
        {
            _logger.LogInformation("Timer Elapsed. Executing cleanup.");

            // Create a new scope for this operation to keep the context alive
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                var (timeCheckFrom, timeCheckUntil) = GetReservationTimeWindow();

                // Pass the context to the repository method
                await _productRepository.DeleteExpiredReservationsAsync(context, timeCheckUntil, timeCheckFrom);
            }

            _logger.LogInformation("RESERVATIONS DELETED");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during timer elapsed : {ErrorMessage}", ex.Message);
        }
        finally
        {
            _logger.LogInformation("Stopping timer for reservations cleanup");
            _timer?.Stop();
        }
    }

    private (DateTime timeCheckFrom, DateTime timeCheckUntil) GetReservationTimeWindow()
    {
        var stockholmTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, stockholmTimeZone);

        var timeCheckFrom = now.AddMinutes(-15.2);
        var timeCheckUntil = now.AddMinutes(0.2);

        return (timeCheckFrom, timeCheckUntil);
    }

}
