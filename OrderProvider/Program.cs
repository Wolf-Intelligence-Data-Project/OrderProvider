using Microsoft.EntityFrameworkCore;
using OrderProvider.Core.Interfaces.Repositories;
using OrderProvider.Core.Interfaces.Services;
using OrderProvider.Core.Repositories;
using OrderProvider.Core.Services;
using OrderProvider.Messaging.AzureServiceBus;
using OrderProvider.Persistence.Data;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDatabase")));

// Register repositories
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IBulkProductUpdateService, BulkProductUpdateService>();

// Register IAzureServiceBusPublisher and its implementation
builder.Services.AddScoped<IAzureServiceBusPublisher, AzureServiceBusPublisher>();

// Register HttpClient for IPaymentService
builder.Services.AddHttpClient();

// Register Azure Service Bus Client
builder.Services.AddSingleton<ServiceBusClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureServiceBusConnectionString");
    return new ServiceBusClient(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
