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
var environmentName = builder.Environment.EnvironmentName;

// Secretos locales por entorno (ignorados por git, nunca al repo)
builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json.local", optional: true);

builder.Logging.AddConsole();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<IOrderNotifier, OrderNotifier>();
builder.Services.AddHttpContextAccessor();

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
            throw new InvalidOperationException($"La cadena de conexión 'SupabaseConnection' está vacía para '{environmentName}'. Copia appsettings.{environmentName}.json.local.example a appsettings.{environmentName}.json.local y llena tus credenciales de Supabase.");

        app.Logger.LogInformation("Intentando validar la conexión de Supabase en entorno '{EnvironmentName}'.", environmentName);

        var canConnect = db.Database.CanConnect();
        if (!canConnect)
            throw new InvalidOperationException(
                "No se pudo establecer conexión con la base de datos de Supabase. " +
                "Verifica que el proyecto no esté pausado (reactívalo en https://supabase.com/dashboard), " +
                "y que Host, Puerto, Usuario, Password, Base de datos y el esquema (uuid/text) sean correctos.");

        app.Logger.LogInformation("Conexión a la base de datos validada correctamente.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al validar la conexión a la base de datos. Verifica ConnectionStrings:SupabaseConnection y el esquema de Postgres (uuid/text). ");
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
