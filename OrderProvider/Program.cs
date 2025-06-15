using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OrderProvider.Interfaces.Repositories;
using OrderProvider.Interfaces.Services;
using OrderProvider.Services;
using OrderProvider.Repositories;
using OrderProvider.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using OrderProvider.Models.Settings;
using OrderProvider.Interfaces;
using OrderProvider.Interfaces.Helpers;
using OrderProvider.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtAccess:Key"])),
            ValidIssuer = builder.Configuration["JwtAccess:Issuer"],
            ValidAudience = builder.Configuration["JwtAccess:Audience"],
            ClockSkew = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.HttpContext.Request.Cookies["AccessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

                if (claimsIdentity == null)
                {
                    context.Fail("Token är ogiltigt. Claims identity är null.");
                    return Task.CompletedTask;
                }

                var isVerifiedClaim = claimsIdentity.FindFirst("isVerified");

                if (isVerifiedClaim == null)
                {
                    context.Fail("Token är ogiltigt. Kontot är inte verifiead.");
                    return Task.CompletedTask;
                }

                if (isVerifiedClaim.Value != "true")
                {
                    context.Fail("Token är ogiltigt. Användaren är inte verifierad.");
                    return Task.CompletedTask;
                }

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validerades framgångsrikt för användare {UserName}.", claimsIdentity.Name);

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError($"Autentisering misslyckades: {context.Exception.Message}");
                if (context.Exception.StackTrace != null)
                {
                    logger.LogError(context.Exception.StackTrace);
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDatabase"))
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging());
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDatabase")), ServiceLifetime.Scoped);

builder.Services.Configure<PriceSettings>(builder.Configuration.GetSection("PriceSettings"));
builder.Services.AddHttpContextAccessor();



builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();

builder.Services.AddScoped<ITokenExtractor, TokenExtractor>(provider =>
{
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var logger = provider.GetRequiredService<ILogger<TokenExtractor>>();
    var jwtSecretKey = builder.Configuration["JwtAccess:Key"];

    return new TokenExtractor(httpContextAccessor, logger, jwtSecretKey);
});
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IKlarnaService, KlarnaService>();

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.SetIsOriginAllowed(_ => true)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
