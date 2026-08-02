using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Restaurante.Api.Hubs;
using Restaurante.Api.Services;
using Restaurante.Application;
using Restaurante.Application.Interfaces;
using Restaurante.Infrastructure;
using Restaurante.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Secretos locales por entorno (ignorados por git, nunca al repo)
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json.local", optional: true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<IOrderNotifier, OrderNotifier>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? string.Empty;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RestauranteDbContext>();
    var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");

    try
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión 'SupabaseConnection' está vacía o no se configuró.");

        var canConnect = db.Database.CanConnect();
        if (!canConnect)
            throw new InvalidOperationException("No se pudo establecer conexión con la base de datos de Supabase.");

        app.Logger.LogInformation("Conexión a la base de datos validada correctamente.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al validar la conexión a la base de datos. Verifica ConnectionStrings:SupabaseConnection.");
        throw;
    }
}

app.UseMiddleware<Restaurante.Api.Middleware.ExceptionMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/orderHub");

app.Run();
