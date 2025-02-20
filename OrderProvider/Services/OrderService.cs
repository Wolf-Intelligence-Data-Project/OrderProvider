using OrderProvider.Entities;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.ServiceBus;

namespace OrderProvider.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceProviderService _invoiceProviderService;
        private readonly IFileProviderService _fileProviderService;
        private readonly IRabbitMqService _rabbitMqService;
        private decimal _currentPricePerProduct = 6m; // Will be updated via admin later

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IPaymentService paymentService,
            IInvoiceProviderService invoiceProviderService,
            IFileProviderService fileProviderService,
            IRabbitMqService rabbitMqService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _paymentService = paymentService;
            _invoiceProviderService = invoiceProviderService;
            _fileProviderService = fileProviderService;
            _rabbitMqService = rabbitMqService;
        }

        public async Task<Guid> CreateOrderAsync(Guid userId, string filtersUsed)
        {
            var reservedProducts = await _productRepository.GetReservedProductsByUserIdAsync(userId);
            if (!reservedProducts.Any()) throw new InvalidOperationException("No reserved products found.");

            int quantity = reservedProducts.Count;
            decimal totalPrice = quantity * _currentPricePerProduct;

            var paymentResult = await _paymentService.ProcessPaymentAsync(userId, totalPrice);
            if (!paymentResult.Success) throw new InvalidOperationException("Payment failed.");

            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                PricePerProductAtPurchase = _currentPricePerProduct,
                Quantity = quantity,
                TotalPrice = totalPrice,
                PaymentStatus = "Completed",
                FiltersUsed = filtersUsed
            };

            await _orderRepository.CreateOrderAsync(order);

            // Publish to RabbitMQ (InvoiceProvider)
            var invoiceEvent = new InvoiceEvent
            {
                OrderId = order.Id,
                UserId = userId,
                TotalPrice = totalPrice
            };
            _rabbitMqService.PublishInvoiceEvent(invoiceEvent);

            // Publish to RabbitMQ (FileProvider)
            var fileEvent = new FileEvent
            {
                UserId = userId,
                OrderId = order.Id
            };
            _rabbitMqService.PublishFileEvent(fileEvent);

            foreach (var product in reservedProducts)
            {
                product.ReservedUntil = null;
                product.ReservedBy = null;
                product.SoldUntil = DateTime.UtcNow.AddDays(30);
                product.SoldTo = userId;
            }

            await _productRepository.BulkUpdateProductsAsync(reservedProducts);
            _rabbitMqService.PublishProductUpdate(reservedProducts);

            return order.Id;
        }

        public async Task<List<OrderEntity>> GetUserOrderHistoryAsync(Guid userId)
        {
            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<List<OrderEntity>> GetAllOrderHistoryAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }
    }
}