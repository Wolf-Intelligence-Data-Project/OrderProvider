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
        return await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<OrderEntity>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await _context.Orders.Where(o => o.CustomerId == userId).ToListAsync();
    }

    public async Task<List<OrderEntity>> GetAllOrdersAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<bool> DeleteOrderAsync(Guid userId)
    {
        var orders = await _context.Orders
                                   .Where(o => o.CustomerId == userId)
                                   .ToListAsync();

        if (orders == null || !orders.Any())
        {
            return false;
        }

        _context.Orders.RemoveRange(orders);

        var rowsAffected = await _context.SaveChangesAsync();

        return rowsAffected > 0;
    }
    public async Task<bool> DeleteUnpaidOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
                                   .Where(o => o.CustomerId == userId && o.PaymentStatus != "Paid")
                                   .ToListAsync();

        if (orders == null || !orders.Any())
        {
            return false;
        }

        _context.Orders.RemoveRange(orders);

        var rowsAffected = await _context.SaveChangesAsync();

        return rowsAffected > 0;
    }

    public async Task<OrderEntity> UpdateOrderAsync(OrderEntity order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
}
