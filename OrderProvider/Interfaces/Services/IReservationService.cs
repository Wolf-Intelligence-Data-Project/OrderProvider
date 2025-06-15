using OrderProvider.Models.DTOs;
using OrderProvider.Models.Requests;

namespace OrderProvider.Interfaces.Services;

public interface IReservationService
{
    Task<ReservationDto> ReserveProductsAsync(ProductReserveRequest request);
    Task<ReservationDto> GetReservationByUserIdAsync();
    Task<bool> DeleteReservationNow();
}
