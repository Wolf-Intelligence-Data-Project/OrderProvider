using Dapper;
using Microsoft.Data.SqlClient;
using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;

namespace OrderProvider.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ProductDatabase");
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
                    WHERE ReservedBy = @UserId AND ReservedUntil IS NOT NULL"; // Changed to "Products"

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
                                ReservedBy = NULL,
                                SoldTo = @SoldTo
                            WHERE Id = @Id"; // Changed to "Products"

                        try
                        {
                            await connection.ExecuteAsync(sql, new
                            {
                                product.Id,
                                product.SoldUntil,
                                product.SoldTo
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
    }
}
