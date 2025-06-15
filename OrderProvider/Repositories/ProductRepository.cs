using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderProvider.Data;
using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Models.Requests;

namespace OrderProvider.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _productDbContext;
    private readonly string _productDbConnectionString;
    private readonly string _reservationDbConnectionString;
    private readonly ILogger<ProductRepository> _logger;
    private readonly DateTime _stockholmTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm"));
    public ProductRepository(IConfiguration configuration, ProductDbContext productDbContext, ILogger<ProductRepository> logger)
    {
        _productDbConnectionString = configuration.GetConnectionString("ProductDatabase");
        _reservationDbConnectionString = configuration.GetConnectionString("OrderDatabase");
        _productDbContext = productDbContext;
        _logger = logger;
    }

    public async Task<ReservationEntity?> GetReservationByIdAsync(Guid reservationId)
    {
        return await _productDbContext.Set<ReservationEntity>()
                              .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
    }

    public async Task<ProductEntity> GetProductByIdAsync(Guid productId)
    {
        using (var connection = new SqlConnection(_productDbConnectionString))
        {
            await connection.OpenAsync();

            string sql = "SELECT * FROM dbo.Products WHERE Id = @ProductId";
            try
            {
                return await connection.QueryFirstOrDefaultAsync<ProductEntity>(sql, new { ProductId = productId });
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error occurred while retrieving product by ID", ex);
            }
        }
    }

    public async Task<List<ProductEntity>> GetReservedProductsByUserIdAsync(Guid userId)
    {
        using (var connection = new SqlConnection(_productDbConnectionString))
        {
            await connection.OpenAsync();

            string sql = @"
                    SELECT * FROM dbo.Products
                    WHERE CustomerId = @UserId AND ReservedUntil IS NOT NULL";

            try
            {
                return (await connection.QueryAsync<ProductEntity>(sql, new { UserId = userId })).AsList();
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error occurred while retrieving reserved products", ex);
            }
        }
    }

    // Bulk updates the products' SoldUntil and resets ReservedUntil
    public async Task BulkUpdateProductsAsync(List<ProductEntity> products)
    {
        using (var connection = new SqlConnection(_productDbConnectionString))
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                foreach (var product in products)
                {
                    string sql = @"
                            UPDATE dbo.Products
                            SET SoldUntil = @SoldUntil,
                                ReservedUntil = NULL,
                                CustomerId = @CustomerId
                            WHERE Id = @Id";

                    try
                    {
                        await connection.ExecuteAsync(sql, new
                        {
                            product.ProductId,
                            product.SoldUntil,
                            product.CustomerId
                        }, transaction);
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Database error occurred during bulk update", ex);
                    }
                }

                await transaction.CommitAsync();
            }
        }
    }

    public async Task ProductSoldAsync(Guid customerId)
    {
        using var connection = new SqlConnection(_productDbConnectionString);

        var soldUntilTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm")).AddDays(30);

        string sql = @"
        UPDATE Products
        SET ReservedUntil = NULL, SoldUntil = @SoldUntil
        WHERE CustomerId = @CustomerId AND ReservedUntil IS NOT NULL";

        var parameters = new DynamicParameters();
        parameters.Add("SoldUntil", soldUntilTime);
        parameters.Add("CustomerId", customerId);

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<List<Guid>> GetProductIdsForReservationAsync(ProductReserveRequest filters, List<string> rawBusinessTypes)
    {
        if (filters.QuantityOfFiltered <= 0)
        {
            _logger.LogWarning("Invalid quantity of products to reserve: {Quantity}", filters.QuantityOfFiltered);
            return new List<Guid>();
        }

        var parameters = new DynamicParameters();
        parameters.Add("Quantity", filters.QuantityOfFiltered);
        parameters.Add("@NowStockholm", _stockholmTime);


        var sql = @"
        SELECT TOP (@Quantity) p.ProductId
        FROM Products p
        WHERE (p.SoldUntil IS NULL OR p.SoldUntil < @NowStockholm)
          AND p.ReservedUntil IS NULL";

        if (rawBusinessTypes?.Any() == true)
        {
            sql += " AND LEFT(p.BusinessType, CHARINDEX('.', p.BusinessType + '.') - 1) IN @BusinessTypes";
            parameters.Add("BusinessTypes", rawBusinessTypes);
        }

        // Add other filters if provided (Cities, PostalCodes, etc.)
        if (filters.Cities?.Any() == true)
        {
            sql += " AND p.City IN @Cities";
            parameters.Add("Cities", filters.Cities);
        }

        if (filters.PostalCodes?.Any() == true)
        {
            sql += " AND p.PostalCode IN @PostalCodes";
            parameters.Add("PostalCodes", filters.PostalCodes);
        }

        if (filters.MinRevenue.HasValue)
        {
            sql += " AND p.Revenue >= @MinRevenue";
            parameters.Add("MinRevenue", filters.MinRevenue);
        }

        if (filters.MinNumberOfEmployees.HasValue)
        {
            sql += " AND p.NumberOfEmployees >= @MinNumberOfEmployees";
            parameters.Add("MinNumberOfEmployees", filters.MinNumberOfEmployees);
        }

        _logger.LogInformation("Generated SQL Query: {SqlQuery}", sql);
        _logger.LogInformation("Query Parameters: Quantity = {Quantity}, BusinessTypes = {BusinessTypes}, Cities = {Cities}, PostalCodes = {PostalCodes}, MinRevenue = {MinRevenue}, MinEmployees = {MinEmployees}",
                               filters.QuantityOfFiltered, string.Join(", ", rawBusinessTypes), string.Join(", ", filters.Cities ?? new List<string>()),
                               string.Join(", ", filters.PostalCodes ?? new List<string>()), filters.MinRevenue, filters.MinNumberOfEmployees);

        sql += " ORDER BY NEWID()";

        using var connection = new SqlConnection(_productDbConnectionString);
        try
        {
            var productIds = (await connection.QueryAsync<Guid>(sql, parameters)).ToList();

            if (productIds.Any())
            {
                _logger.LogInformation("Found {ProductCount} product IDs for reservation.", productIds.Count);
            }
            else
            {
                _logger.LogWarning("No products found for reservation with the given filters.");
            }

            return productIds;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error executing query for fetching product IDs: {ErrorMessage}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return new List<Guid>();  // Return an empty list if query fails
        }
    }


    public async Task<IEnumerable<ProductEntity>> GetProductsByCustomerIdAsync(Guid customerId)
    {
        return await _productDbContext.Products
            .Where(p => p.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task UpdateProductAsync(ProductEntity product)
    {
        _productDbContext.Products.Update(product);
        await _productDbContext.SaveChangesAsync();
    }

    #region Reserving for Productstable

    public async Task ReserveProductsByIdsAsync(List<Guid> productIds, Guid userId)
    {
        _logger.LogInformation("Starting ReserveProductsByIdsAsync...");

        if (productIds == null || !productIds.Any())
        {
            _logger.LogWarning("No product IDs provided. Skipping reservation.");
            return;
        }

        var reservedUntil = TimeZoneInfo
            .ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm"))
            .AddMinutes(15);

        _logger.LogInformation("Reserving products for user: {UserId}", userId);
        _logger.LogInformation("Products to reserve: {@ProductIds}", productIds);
        _logger.LogInformation("ReservedUntil set to: {ReservedUntil}", reservedUntil);

        var sql = @"
        UPDATE Products
        SET CustomerId = @CustomerId, ReservedUntil = @ReservedUntil
        WHERE ProductId IN @ProductIds";

        var parameters = new DynamicParameters();
        parameters.Add("CustomerId", userId);
        parameters.Add("ReservedUntil", reservedUntil);
        parameters.Add("ProductIds", productIds);

        try
        {
            using var connection = new SqlConnection(_productDbConnectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, parameters);

            _logger.LogInformation("Rows affected: {RowsAffected}", rowsAffected);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No rows were updated. Make sure product IDs exist and match.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reserving products.");
            throw;
        }

        _logger.LogInformation("Finished ReserveProductsByIdsAsync.");
    }


    public async Task DeleteExpiredReservationsAsync(ProductDbContext context, DateTime timeCheckUntil, DateTime timeCheckFrom)
    {
        // First DB connection for Products
        using var productConnection = new SqlConnection(_productDbConnectionString);
        await productConnection.OpenAsync();
        using var productTransaction = await productConnection.BeginTransactionAsync();

        try
        {
            string updateProductsSql = @"
        UPDATE [dbo].[Products]
        SET ReservedUntil = NULL, CustomerId = NULL
        WHERE ReservedUntil < @TimeCheckUntil";

            int rowsAffected = await productConnection.ExecuteAsync(updateProductsSql, new { TimeCheckUntil = timeCheckUntil }, productTransaction);

            await productTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await productTransaction.RollbackAsync();
            Console.WriteLine($"Product DB Error: {ex.Message}");
            throw;
        }

        // Second DB connection for Reservations
        using var reservationConnection = new SqlConnection(_reservationDbConnectionString);
        await reservationConnection.OpenAsync();

        try
        {
            string deleteReservationsSql = @"
        DELETE FROM [dbo].[Reservations]
        WHERE ReservedFrom < @TimeCheckFrom";

            int deletedRows = await reservationConnection.ExecuteAsync(deleteReservationsSql, new { TimeCheckFrom = timeCheckFrom });

            if (deletedRows == 0)
            {
                Console.WriteLine("No expired reservations were deleted.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reservation DB Error: {ex.Message}");
            throw;
        }
    }


    public async Task RemoveReservationsAsync(Guid companyId)
    {
        try
        {
            using var connection = new SqlConnection(_productDbConnectionString);

            string sql = @"
                UPDATE Products
                SET ReservedUntil = NULL, CustomerId = NULL
                WHERE CustomerId = @CompanyId AND ReservedUntil IS NOT NULL";

            var parameters = new { CompanyId = companyId };

            await connection.ExecuteAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservations.");
            throw;
        }
    }


    #endregion
}
