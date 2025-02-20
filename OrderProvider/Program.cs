using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.Services;
using OrderProvider.Repositories;
using OrderProvider.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDatabase")));

// Register Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Register Services
builder.Services.AddScoped<IOrderService, OrderService>();

// Register the HttpClient factory
builder.Services.AddHttpClient();  // This is the key line

builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();

// Register IPaymentService and its implementation
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register the invoice and file provider services
builder.Services.AddScoped<IInvoiceProviderService, InvoiceProviderService>();
builder.Services.AddScoped<IFileProviderService, FileProviderService>();

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
