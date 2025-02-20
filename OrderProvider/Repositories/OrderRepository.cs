using Microsoft.EntityFrameworkCore;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Data;
using OrderProvider.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<OrderEntity>> GetOrdersByUserIdAsync(Guid userId) // Implemented
        {
            return await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
        }

        public async Task<List<OrderEntity>> GetAllOrdersAsync() // Implemented
        {
            return await _context.Orders.ToListAsync();
        }
    }
}
