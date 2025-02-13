using OrderProvider.Data;
using OrderProvider.Models;

namespace OrderProvider.Repositories;


public class OrderRepository
{
    private readonly OrderDbContext _context;
    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    // Add other methods like GetOrder, UpdateOrder, etc.
}
