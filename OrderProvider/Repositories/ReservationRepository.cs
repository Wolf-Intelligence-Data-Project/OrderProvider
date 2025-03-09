using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderProvider.Data;
using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;

namespace OrderProvider.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ProductDbContext _context;
    private readonly string _connectionString;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(ProductDbContext context, IConfiguration configuration, ILogger<ReservationRepository> logger)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("ProductDatabase");
        _logger = logger;
    }

    #region Reservations Table
    // This part communicates with products table
    public async Task AddReservationAsync(ReservationEntity reservation)
    {
        await _context.Set<ReservationEntity>().AddAsync(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task<ReservationEntity> GetReservationByUserIdAsync(Guid customerId)
    {
        return await _context.Set<ReservationEntity>()
                             .FirstOrDefaultAsync(r => r.CustomerId == customerId && r.ReservedFrom != null && r.SoldFrom == null);
    }
    public async Task DeleteReservationImmediatelyAsync(Guid reservationId)
    {
        var reservation = await _context.Set<ReservationEntity>().FindAsync(reservationId);
        if (reservation != null)
        {
            _context.Set<ReservationEntity>().Remove(reservation);
            await _context.SaveChangesAsync();
        }
    }

    // Fetch reservation by customer ID
    public async Task<ReservationEntity> GetReservationByCustomerIdAsync(Guid customerId)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.CustomerId == customerId);
    }

    // Update reservation entity
    public async Task UpdateReservationAsync(ReservationEntity reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateToSoldAsync(Guid reservationId)
    {
        try
        {
            // Find the reservation by ReservationId
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation != null)
            {
                // Manually setting the values
                reservation.ReservedFrom = null;  // Set to null or appropriate logic
                reservation.SoldFrom = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm"));

                // Save changes to the database
                _context.Update(reservation);  // This marks the entire entity for update

                // Explicitly mark the specific properties as modified
                _context.Entry(reservation).Property(r => r.ReservedFrom).IsModified = true;
                _context.Entry(reservation).Property(r => r.SoldFrom).IsModified = true;

                // Save changes to the database
                await _context.SaveChangesAsync();
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
            // Open the connection
            await connection.OpenAsync();

            // SQL query to update the specified fields
            var query = @"
            UPDATE Products
            SET SoldUntil = NULL,
                CustomerId = NULL,
                ReservedUntil = NULL
            WHERE SoldUntil IS NOT NULL";

            // Execute the query and return the number of affected rows
            return await connection.ExecuteAsync(query);
        }
    }

    public async Task<ReservationEntity> GetReservationByIdAsync(Guid reservationId)
    {
        try
        {
            // Query the database to find the reservation by its ReservationId
            var reservation = await _context.Set<ReservationEntity>()
                                            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            return reservation;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as necessary
            throw new Exception("Error retrieving reservation by ID", ex);
        }
    }
    #endregion
}
