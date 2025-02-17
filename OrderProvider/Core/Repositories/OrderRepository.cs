using Microsoft.EntityFrameworkCore;
using OrderProvider.Core.Entities;
using OrderProvider.Core.Interfaces.Repositories;
using OrderProvider.Persistence.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProvider.Core.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.CartItems) // Include cart items
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetOrdersByCompanyIdAsync(int companyId)
        {
            return await _context.Orders
                .Include(o => o.CartItems)
                .Where(o => o.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderPaymentStatusAsync(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            order.PaymentStatus = paymentStatus;
            if (paymentStatus == "Paid")
                order.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
