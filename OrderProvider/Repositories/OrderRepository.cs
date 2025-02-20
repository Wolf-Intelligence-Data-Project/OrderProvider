using Microsoft.EntityFrameworkCore;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Data;
using OrderProvider.Entities;

namespace OrderProvider.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<OrderEntity> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Guid> CreateOrderAsync(OrderEntity order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order.Id;
        }
    }
}
