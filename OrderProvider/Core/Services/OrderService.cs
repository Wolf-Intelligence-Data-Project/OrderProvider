using OrderProvider.Core.DTOs;
using OrderProvider.Core.Entities;
using OrderProvider.Core.Interfaces.Repositories;
using OrderProvider.Core.Interfaces.Services;

namespace OrderProvider.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IBulkProductUpdateService _bulkProductUpdateService;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IBulkProductUpdateService bulkProductUpdateService)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _bulkProductUpdateService = bulkProductUpdateService;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(request.UserId);
            if (cart == null || !cart.CartItems.Any())
                throw new Exception("Cart is empty or not found.");

            var order = new Order
            {
                CompanyId = request.CompanyId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = cart.CartItems.Count() * 6m,
                PaymentStatus = "Pending",
                CartId = cart.Id,  // Set CartId properly
                CartItems = cart.CartItems.ToList() // Populate CartItems from Cart
            };

            var createdOrder = await _orderRepository.CreateOrderAsync(order);
            return createdOrder;
        }


        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<bool> ConfirmPaymentAsync(int orderId)
        {
            // Update order payment status
            var success = await _orderRepository.UpdateOrderPaymentStatusAsync(orderId, "Paid");
            if (!success) return false;

            // Retrieve order details
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null) return false;

            // Mark products as sold
            var productIds = order.CartItems.Select(c => c.ProductId).ToList();
            await _bulkProductUpdateService.MarkProductsAsSoldAsync(productIds, DateTime.UtcNow.AddHours(24));

            return true;
        }
    }
}
