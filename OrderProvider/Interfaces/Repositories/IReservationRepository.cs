using OrderProvider.Data;
using OrderProvider.Entities;
using OrderProvider.Models.DTOs;

namespace OrderProvider.Interfaces.Repositories;

public interface IReservationRepository
{
    Task AddReservationAsync(ReservationEntity reservation);
    Task UpdateToSoldAsync(Guid reservationId);
    Task DeleteReservationImmediatelyAsync(Guid reservationId);
    Task<ReservationEntity> GetReservationByIdAsync(Guid reservationId);

    Task<ReservationEntity> GetReservationByCustomerIdAsync(Guid customerId);
    Task<ReservationEntity> GetReservationAsync(Guid customerId);
    Task<int> DeleteSoldProductsAsync();
}
