namespace OrderProvider.Factories;

using OrderProvider.Entities;
using OrderProvider.Models.DTOs;
using OrderProvider.Models.Requests;

public class ReservationFactory
{
    public static ReservationEntity CreateReservationEntity(ProductReserveRequest request)
    {
        return new ReservationEntity
        {
            ReservationId = Guid.NewGuid(),
            CustomerId = request.CompanyId,
            BusinessTypes = string.Join(",", request.BusinessTypes ?? new List<string>()),
            Regions = string.Join(",", request.Regions ?? new List<string>()),
            Cities = request.Cities != null ? string.Join(",", request.Cities) : null,  // Separate Cities
            CitiesByRegion = request.CitiesByRegion != null ? string.Join(",", request.CitiesByRegion) : null, // Separate CitiesByRegion
            PostalCodes = string.Join(",", request.PostalCodes ?? new List<string>()),
            MinRevenue = request.MinRevenue,
            MaxRevenue = request.MaxRevenue,
            MinNumberOfEmployees = request.MinNumberOfEmployees,
            MaxNumberOfEmployees = request.MaxNumberOfEmployees,
            Quantity = request.QuantityOfFiltered,
            ReservedFrom = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm")),
            SoldFrom = null,
        };
    }
    public static ReservationDto CreateReservationDto(ReservationEntity reservation)
    {
        return new ReservationDto
        {
            CustomerId = reservation.CustomerId,
            ReservationId = reservation.ReservationId,
            BusinessTypes = reservation.BusinessTypes,
            Regions = reservation.Regions,
            Cities = reservation.Cities,
            CitiesByRegion = reservation.CitiesByRegion,
            PostalCodes = reservation.PostalCodes,
            MinRevenue = reservation.MinRevenue,
            MaxRevenue = reservation.MaxRevenue,
            MinNumberOfEmployees = reservation.MinNumberOfEmployees,
            MaxNumberOfEmployees = reservation.MaxNumberOfEmployees,
            Quantity = reservation.Quantity,
            ReservedFrom = reservation.ReservedFrom,
            SoldFrom = reservation.SoldFrom,
        };
    }
}
