using Microsoft.EntityFrameworkCore;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Data;
using OrderProvider.Entities;

namespace OrderProvider.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }
    public async Task CreateOrderAsync(OrderEntity order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }
    public async Task<OrderEntity> GetOrderByIdAsync(Guid orderId)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
    }

    //public async Task<Guid> CreateOrderAsync(OrderEntity order)
    //{
    //    _context.Orders.Add(order);
    //    await _context.SaveChangesAsync();
    //    return order.Id;
    //}

    public async Task<List<OrderEntity>> GetOrdersByUserIdAsync(Guid userId) // Implemented
    {
        return await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
    }

    public async Task<List<OrderEntity>> GetAllOrdersAsync() // Implemented
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<bool> DeleteOrderAsync(Guid userId)
    {
        // Find orders by UserId
        var orders = await _context.Orders
                                   .Where(o => o.UserId == userId)
                                   .ToListAsync();

        if (orders == null || !orders.Any())
        {
            // If no orders found, return false
            return false;
        }

        // Remove the found orders from the context
        _context.Orders.RemoveRange(orders);

        // Save the changes to the database
        var rowsAffected = await _context.SaveChangesAsync();

        // If rowsAffected is greater than 0, the deletion was successful
        return rowsAffected > 0;
    }


}
