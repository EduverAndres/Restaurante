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
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        if (isDev)
        {
            services.AddDbContext<RestauranteDbContext>(o =>
                o.UseSqlite("Data Source=restaurante.db"));
        }
        else
        {
            services.AddDbContext<RestauranteDbContext>(o =>
                o.UseNpgsql(config.GetConnectionString("SupabaseConnection")));
        }

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAIConversationRepository, AIConversationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();

        services.AddSingleton<RefreshTokenStore>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddHttpClient<IPaymentService, PaymentService>();
        services.AddHttpClient<IAIService, AIService>();

        return services;
    }
}
