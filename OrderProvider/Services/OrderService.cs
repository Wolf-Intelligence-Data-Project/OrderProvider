using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;

namespace OrderProvider.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private const decimal PricePerProduct = 6m;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<Guid> CreateOrderAsync(Guid userId)
        {
            var reservedProducts = await _productRepository.GetReservedProductsByUserIdAsync(userId);
            if (!reservedProducts.Any()) throw new InvalidOperationException("No reserved products found for this user.");

            var totalPrice = reservedProducts.Count * PricePerProduct;

            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = totalPrice,
                PaymentStatus = "Pending"
            };

            var orderId = await _orderRepository.CreateOrderAsync(order);

            foreach (var product in reservedProducts)
            {
                product.ReservedUntil = null;
                product.ReservedBy = null;
            }

            await _productRepository.BulkUpdateProductsAsync(reservedProducts);
            return orderId;
        }

        public async Task<bool> CompleteOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null) return false;

            var products = await _productRepository.GetReservedProductsByUserIdAsync(order.UserId);
            if (!products.Any()) return false;

            foreach (var product in products)
            {
                product.SoldUntil = DateTime.UtcNow.AddDays(30);
                product.ReservedUntil = null;
                product.ReservedBy = null;
                product.SoldTo = order.UserId;
            }

            await _productRepository.BulkUpdateProductsAsync(products);
            return true;
        }
    }
}
