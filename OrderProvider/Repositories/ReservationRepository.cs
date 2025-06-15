using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderProvider.Data;
using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;

namespace OrderProvider.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly OrderDbContext _orderDbContext;
    private readonly string _connectionString;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(OrderDbContext orderDbContext, IConfiguration configuration, ILogger<ReservationRepository> logger)
    {
        _orderDbContext = orderDbContext;
        _connectionString = configuration.GetConnectionString("ProductDatabase");
        _logger = logger;
    }

    #region Reservations Table

    public async Task AddReservationAsync(ReservationEntity reservation)
    {
        _logger.LogInformation("DB: {db}, Source: {source}",
    _orderDbContext.Database.GetDbConnection().Database,
    _orderDbContext.Database.GetDbConnection().DataSource);
        await _orderDbContext.Set<ReservationEntity>().AddAsync(reservation);
        await _orderDbContext.SaveChangesAsync();
    }

    public async Task<ReservationEntity> GetReservationAsync(Guid customerId)
    {
        var reservation = await _orderDbContext.Reservations
            .Where(r => r.CustomerId == customerId && r.ReservedFrom != null && r.SoldFrom == null)
            .FirstOrDefaultAsync();

        if (reservation == null)
        {
            _logger.LogWarning("No reservation found for customerId: {CustomerId}", customerId);
        }

        return reservation;
    }

    public async Task DeleteReservationImmediatelyAsync(Guid reservationId)
    {
        _logger.LogInformation("Attempting to delete reservation with ID: {ReservationId}", reservationId);

        var reservation = await _orderDbContext.Reservations.FindAsync(reservationId);
        if (reservation != null)
        {
            _orderDbContext.Set<ReservationEntity>().Remove(reservation);
            await _orderDbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted reservation with ID: {ReservationId}", reservationId);
        }
        else
        {
            _logger.LogWarning("Reservation with ID: {ReservationId} not found. Nothing to delete.", reservationId);
        }
    }

    public async Task<ReservationEntity> GetReservationByCustomerIdAsync(Guid customerId)
    {
        return await _orderDbContext.Reservations
            .FirstOrDefaultAsync(r => r.CustomerId == customerId);
    }

    public async Task UpdateReservationAsync(ReservationEntity reservation)
    {
        _orderDbContext.Reservations.Update(reservation);
        await _orderDbContext.SaveChangesAsync();
    }

    public async Task UpdateToSoldAsync(Guid reservationId)
    {
        try
        {
            var reservation = await _orderDbContext.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation != null)
            {
                reservation.ReservedFrom = null; 
                reservation.SoldFrom = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm"));


                _orderDbContext.Update(reservation);

                _orderDbContext.Entry(reservation).Property(r => r.ReservedFrom).IsModified = true;
                _orderDbContext.Entry(reservation).Property(r => r.SoldFrom).IsModified = true;

                await _orderDbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Reservation not found with ReservationId: {ReservationId}", reservationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation to sold for ReservationId: {ReservationId}", reservationId);
        }
    }

    public async Task<int> DeleteSoldProductsAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {

            await connection.OpenAsync();

            var query = @"
            UPDATE Products
            SET SoldUntil = NULL,
                CustomerId = NULL,
                ReservedUntil = NULL
            WHERE SoldUntil IS NOT NULL";

            return await connection.ExecuteAsync(query);
        }
    }

    public async Task<ReservationEntity> GetReservationByIdAsync(Guid reservationId)
    {
        try
        {
            var reservation = await _orderDbContext.Set<ReservationEntity>()
                                            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            return reservation;
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving reservation by ID", ex);
        }
    }
    #endregion
}
