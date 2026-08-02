using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Restaurante.Application.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Infrastructure.Repositories;
using Restaurante.Infrastructure.Services;

namespace Restaurante.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("SupabaseConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Fallback local sin credenciales configuradas (ej. tests o primer arranque)
            services.AddDbContext<RestauranteDbContext>(o =>
                o.UseSqlite("Data Source=restaurante.db"));
        }
        else
        {
            services.AddDbContext<RestauranteDbContext>(o =>
                o.UseNpgsql(connectionString));
        }

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IBusinessHourRepository, BusinessHourRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAIConversationRepository, AIConversationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IRiderRepository, RiderRepository>();

        services.AddSingleton<RefreshTokenStore>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddHttpClient<IPaymentService, PaymentService>();
        services.AddHttpClient<IAIService, AIService>();
        services.AddHttpClient<IStorageService, StorageService>();

        return services;
    }
}
