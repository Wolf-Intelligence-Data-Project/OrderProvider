using OrderProvider.Models;
using OrderProvider.Repositories;

namespace OrderProvider.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly CartRepository _cartRepository;
    private readonly ProductRepository _productRepository;

    public OrderService(OrderRepository orderRepository, CartRepository cartRepository, ProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<Order> PlaceOrderAsync(int cartId)
    {
        var cartItems = await _cartRepository.GetCartItemsAsync(cartId);
        var order = new Order
        {
            CreatedAt = DateTime.Now,
            OrderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };

        return await _orderRepository.CreateOrderAsync(order);
    }
}
