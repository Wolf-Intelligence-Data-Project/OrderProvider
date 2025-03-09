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
    private readonly string _connectionString;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(IConfiguration configuration, ProductDbContext productDbContext, ILogger<ProductRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("ProductDatabase");
        _productDbContext = productDbContext;
        _logger = logger;
    }

    public async Task<ReservationEntity?> GetReservationByIdAsync(Guid reservationId)
    {
        return await _productDbContext.Set<ReservationEntity>()
                              .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
    }


    // Retrieves a product by its ID
    public async Task<ProductEntity> GetProductByIdAsync(Guid productId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Ensure the schema is specified if necessary (e.g., dbo)
            string sql = "SELECT * FROM dbo.Products WHERE Id = @ProductId"; // Changed to "Products"
            try
            {
                return await connection.QueryFirstOrDefaultAsync<ProductEntity>(sql, new { ProductId = productId });
            }
            catch (SqlException ex)
            {
                // Log the error or handle it
                throw new Exception("Database error occurred while retrieving product by ID", ex);
            }
        }
    }

    // Retrieves all reserved products for a specific user
    public async Task<List<ProductEntity>> GetReservedProductsByUserIdAsync(Guid userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Ensure the schema is specified if necessary (e.g., dbo)
            string sql = @"
                    SELECT * FROM dbo.Products
                    WHERE CustomerId = @UserId AND ReservedUntil IS NOT NULL"; // Changed to "Products"

            try
            {
                return (await connection.QueryAsync<ProductEntity>(sql, new { UserId = userId })).AsList();
            }
            catch (SqlException ex)
            {
                // Log the error or handle it
                throw new Exception("Database error occurred while retrieving reserved products", ex);
            }
        }
    }

    // Bulk updates the products' SoldUntil and resets ReservedUntil
    public async Task BulkUpdateProductsAsync(List<ProductEntity> products)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                foreach (var product in products)
                {
                    // Ensure the schema is specified if necessary (e.g., dbo)
                    string sql = @"
                            UPDATE dbo.Products
                            SET SoldUntil = @SoldUntil,
                                ReservedUntil = NULL,
                                CustomerId = @CustomerId
                            WHERE Id = @Id"; // Changed to "Products"

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
                        // Log the error or handle it, and possibly rollback the transaction
                        await transaction.RollbackAsync();
                        throw new Exception("Database error occurred during bulk update", ex);
                    }
                }

                // Commit the transaction if all queries were successful
                await transaction.CommitAsync();
            }
        }
    }

    public async Task ProductSoldAsync(Guid customerId)
    {
        using var connection = new SqlConnection(_connectionString);

        // Time Zone adjustment for SoldUntil
        var soldUntilTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm")).AddDays(30);

        string sql = @"
            UPDATE Products
            SET ReservedUntil = NULL, SoldUntil = @SoldUntil
            WHERE CustomerId = @CustomerId";

        var parameters = new DynamicParameters();
        parameters.Add("SoldUntil", soldUntilTime);
        parameters.Add("CustomerId", customerId);

        // Execute the SQL command
        await connection.ExecuteAsync(sql, parameters);
    }
    public async Task<List<Guid>> GetProductIdsForReservationAsync(ProductReserveRequest filters, List<string> rawBusinessTypes)
    {
        // Validate quantity
        if (filters.QuantityOfFiltered <= 0)
        {
            _logger.LogWarning("Invalid quantity of products to reserve: {Quantity}", filters.QuantityOfFiltered);
            return new List<Guid>();
        }

        // Build the base SQL query with filters dynamically
        var sql = @"
            SELECT TOP (@Quantity) p.ProductId
            FROM Products p
            WHERE (p.SoldUntil IS NULL AND p.ReservedUntil IS NULL) 
            AND LEFT(p.BusinessType, CHARINDEX('.', p.BusinessType + '.') - 1) IN @BusinessTypes";

        var parameters = new DynamicParameters();
        parameters.Add("Quantity", filters.QuantityOfFiltered);

        // Add the rawBusinessTypes filter if provided
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

        // Log the generated SQL query and parameters for debugging
        _logger.LogInformation("Generated SQL Query: {SqlQuery}", sql);
        _logger.LogInformation("Query Parameters: Quantity = {Quantity}, BusinessTypes = {BusinessTypes}, Cities = {Cities}, PostalCodes = {PostalCodes}, MinRevenue = {MinRevenue}, MinEmployees = {MinEmployees}",
                               filters.QuantityOfFiltered, string.Join(", ", rawBusinessTypes), string.Join(", ", filters.Cities ?? new List<string>()),
                               string.Join(", ", filters.PostalCodes ?? new List<string>()), filters.MinRevenue, filters.MinNumberOfEmployees);

        // Randomize the order before selecting TOP (@Quantity)
        sql += " ORDER BY NEWID()";

        using var connection = new SqlConnection(_connectionString);
        try
        {
            var productIds = (await connection.QueryAsync<Guid>(sql, parameters)).ToList();

            // Log the fetched product IDs
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

    // Update product entity
    public async Task UpdateProductAsync(ProductEntity product)
    {
        _productDbContext.Products.Update(product);
        await _productDbContext.SaveChangesAsync();
    }

    #region Reserving for Productstable
    // This part communicates with products table
    public async Task ReserveProductsByIdsAsync(List<Guid> productIds, Guid companyId)
    {
        var sql = @"
        UPDATE Products
        SET CustomerId = @CompanyId, ReservedUntil = @ReservedUntil
        WHERE ProductId IN @ProductIds";

        var parameters = new DynamicParameters();
        parameters.Add("CompanyId", companyId);
        parameters.Add("ReservedUntil", TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm")).AddMinutes(15));
        parameters.Add("ProductIds", productIds); // Pass the list of GUIDs directly

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, parameters);
    }


    public async Task DeleteExpiredReservationsAsync(ProductDbContext context, DateTime timeCheckUntil, DateTime timeCheckFrom)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Update expired products
            string updateProductsSql = @"
            UPDATE Products
            SET ReservedUntil = NULL, CustomerId = NULL
            WHERE ReservedUntil < @TimeCheckUntil";

            int rowsAffected = await connection.ExecuteAsync(updateProductsSql, new { TimeCheckUntil = timeCheckUntil }, transaction);

            // Check if any rows were updated
            if (rowsAffected == 0)
            {
                Console.WriteLine("No expired products were updated.");
            }


            // Delete expired reservations for the given company
            string deleteReservationsSql = @"
            DELETE FROM Reservations
            WHERE ReservedFrom < @TimeCheckFrom";

            int deletedRows = await connection.ExecuteAsync(deleteReservationsSql, new { TimeCheckFrom = timeCheckFrom }, transaction);

            // Optionally, check if any reservations were deleted
            if (deletedRows == 0)
            {
                Console.WriteLine("No expired reservations were deleted.");
            }

            // Commit the transaction
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            // Rollback the transaction in case of an error
            await transaction.RollbackAsync();
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    public async Task RemoveReservationsAsync(Guid companyId)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Products
                SET ReservedUntil = NULL, CustomerId = NULL
                WHERE CustomerId = @CompanyId AND ReservedUntil IS NOT NULL";

            var parameters = new { CompanyId = companyId };

            // Execute the update query
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
