using Microsoft.Extensions.Options;
using OrderProvider.Data;
using OrderProvider.Factories;
using OrderProvider.Helpers;
using OrderProvider.Interfaces.Helpers;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenExtractor _tokenExtractor;
    private readonly IReservationRepository _reservationRepository;
    private readonly IProductRepository _productRepository;
    private System.Timers.Timer _timer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReservationService> _logger;
    private readonly IOptions<PriceSettings> _priceSettings;

    public ReservationService(ITokenExtractor tokenExtractor, IReservationRepository reservationRepository, IProductRepository productRepository, IServiceScopeFactory serviceScopeFactory, IOptions<PriceSettings> priceSettings, ILogger<ReservationService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _tokenExtractor = tokenExtractor;
        _reservationRepository = reservationRepository;
        _productRepository = productRepository;
        _serviceScopeFactory = serviceScopeFactory;
        _priceSettings = priceSettings;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ReservationDto> ReserveProductsAsync(ProductReserveRequest request)
    {
        var userId = _tokenExtractor.GetUserIdFromToken();

        if (userId == null)
        {
            _logger.LogWarning("Invalid or missing userId in cookie.");
            return null;
        }
        var quantity = request.QuantityOfFiltered;

        _logger.LogInformation("ReserveProductsAsync started for CompanyId: {CompanyId} with Quantity: {Quantity}", userId, quantity);

        if (quantity > 0 && userId != null)
        {
            try
            {
                _logger.LogInformation("Deleting existing reservations for CompanyId: {CompanyId}", userId);
                await DeleteReservationNow();

                _logger.LogInformation("Fetching product IDs for reservation...");

                var (rawBusinessTypes, formattedBusinessTypes) = FormatBusinessTypes(request.BusinessTypes);

                _logger.LogInformation("Formatted business types: {BusinessTypes}", string.Join(", ", formattedBusinessTypes));

                request.BusinessTypes = formattedBusinessTypes;

                var productIds = await _productRepository.GetProductIdsForReservationAsync(request, rawBusinessTypes);
                if (productIds == null || !productIds.Any())
                {
                    _logger.LogWarning("No products found for reservation.");
                }

                _logger.LogInformation("Found {ProductCount} products for reservation.", productIds?.Count());

                _logger.LogInformation("Reserving {ProductCount} products for CompanyId: {CompanyId}", productIds.Count(), userId);
                await _productRepository.ReserveProductsByIdsAsync(productIds, userId);

                var reservation = ReservationFactory.CreateReservationEntity(request, userId);
                _logger.LogInformation("Created reservation with ID: {ReservationId}", reservation);

                await _reservationRepository.AddReservationAsync(reservation);
                _logger.LogInformation("Reservation saved to database with ID: {ReservationId}", reservation);

                StartReservationReleaseTimer();
                _logger.LogInformation("Reservation release timer started.");

                return ReservationFactory.CreateReservationDto(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during reservation process for CompanyId: {CompanyId}: {ErrorMessage}", userId, ex.Message);
                _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
                return null!;
            }
        }

        _logger.LogWarning("Reservation not processed. Invalid quantity or company ID. CompanyId: {CompanyId}, Quantity: {Quantity}", userId, quantity);
        return null!;
    }

    private (List<string> rawBusinessTypes, List<string> formattedBusinessTypes) FormatBusinessTypes(List<string>? businessTypes)
    {
        if (businessTypes == null || !businessTypes.Any())
            return (new List<string>(), new List<string>());

        // Create formatted business types for saving in the database
        var formattedBusinessTypes = businessTypes.Select(code =>
        {
            var trimmedCode = code.Trim().Split('.')[0];

            if (Enum.TryParse(trimmedCode, out BusinessType businessType))
            {
                var description = businessType.GetType()
                                              .GetField(businessType.ToString())
                                              .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                              .FirstOrDefault() as DescriptionAttribute;
                return description != null ? $"{description.Description} ({trimmedCode})" : trimmedCode;
            }
            return trimmedCode;
        }).ToList();

        // Extract raw business types (before the dot) for querying, and add space after the first letter
        var rawBusinessTypes = businessTypes.Select(code =>
        {
            var trimmedCode = code.Trim().Split('.')[0];

            // Add space after the first letter if there's a number following it
            if (trimmedCode.Length > 1 && char.IsLetter(trimmedCode[0]) && char.IsDigit(trimmedCode[1]))
            {
                trimmedCode = trimmedCode.Insert(1, " "); 
            }

            return trimmedCode;
        }).ToList();

        // Return both raw codes for querying and formatted codes for saving
        return (rawBusinessTypes, formattedBusinessTypes);
    }


    private (decimal priceWithoutVat, decimal totalPrice) CalculatePrice(int count)
    {
        decimal pricePerProduct = _priceSettings.Value.PricePerProduct; // SEK per product
        decimal vatRate = _priceSettings.Value.VatRate; // VAT rate (e.g., 25 for 25%)

        if (pricePerProduct <= 0 || vatRate <= 0)
        {
            _logger.LogError("Invalid price settings: PricePerProduct = {PricePerProduct}, VatRate = {VatRate}",
                pricePerProduct, vatRate);
            throw new InvalidOperationException("Invalid price settings.");
        }

        decimal baseTotalPrice = count * pricePerProduct;

        decimal totalPriceWithVat = baseTotalPrice * (1 + vatRate / 100); // Apply VAT to total price

        return (baseTotalPrice, totalPriceWithVat);
    }

    public async Task<ReservationDto> GetReservationByUserIdAsync()
    {
        var userId = _tokenExtractor.GetUserIdFromToken();

        if (userId == null)
        {
            _logger.LogWarning("Invalid or missing userId in cookie.");
            return null;
        }
        var reservation = await _reservationRepository.GetReservationAsync(userId);

        if (reservation == null)
        {
            _logger.LogWarning("No reservation found for user ID: {UserId}", userId);
            return null;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            var (timeCheckFrom, timeCheckUntil) = GetReservationTimeWindow();

            await _productRepository.DeleteExpiredReservationsAsync(context, timeCheckUntil, timeCheckFrom);

            var (priceWithoutVat, totalPriceWithVat) = CalculatePrice(reservation.Quantity);

            var reservationDto = ReservationFactory.CreateReservationDto(reservation);

            reservationDto.PriceWithoutVat = priceWithoutVat;
            reservationDto.TotalPrice = totalPriceWithVat;

            return reservationDto;
        }
    }

    private async Task<bool> DeleteReservationByUserIdAsync(Guid companyId)
    {
        var userId = _tokenExtractor.GetUserIdFromToken();

        if (userId == null)
        {
            _logger.LogWarning("Invalid or missing userId in cookie.");
            return false;
        }
        var reservation = await _reservationRepository.GetReservationAsync(userId);
        if (reservation != null)
        {
            await _reservationRepository.DeleteReservationImmediatelyAsync(reservation.ReservationId);
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteReservationNow()
    {
        var userId = _tokenExtractor.GetUserIdFromToken();

        if (userId == null)
        {
            _logger.LogWarning("Invalid or missing userId in cookie.");
            return false;
        }
        try
        {
            await _productRepository.RemoveReservationsAsync(userId);
            await DeleteReservationByUserIdAsync(userId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void StartReservationReleaseTimer()
    {
        _timer?.Dispose();
        _timer = new System.Timers.Timer(903000);
        _timer.Elapsed += async (sender, e) => await TimerElapsedAsync();
        _timer.AutoReset = false;
        _timer.Start();
        _logger.LogInformation("TIMER STARTED for for reservations cleanup");
    }

    private async Task TimerElapsedAsync()
    {
        try
        {
            _logger.LogInformation("Timer Elapsed. Executing cleanup.");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                var (timeCheckFrom, timeCheckUntil) = GetReservationTimeWindow();

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
