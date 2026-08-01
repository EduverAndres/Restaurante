using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Infrastructure.Data;

public class RestauranteDbContext : DbContext
{
    public RestauranteDbContext(DbContextOptions<RestauranteDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AIConversation> AIConversations => Set<AIConversation>();
    public DbSet<Rider> Riders => Set<Rider>();
    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var isPostgres = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(50);

            entity.HasMany(e => e.Restaurants).WithOne(e => e.Owner).HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Orders).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.AiConversations).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Riders).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.CustomerAddresses).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Reviews).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            if (isPostgres) entity.Property(e => e.ThemeConfig).HasColumnType("jsonb");

            entity.HasMany(e => e.Categories).WithOne(e => e.Restaurant).HasForeignKey(e => e.RestaurantId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.MenuItems).WithOne(e => e.Restaurant).HasForeignKey(e => e.RestaurantId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Orders).WithOne(e => e.Restaurant).HasForeignKey(e => e.RestaurantId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.BusinessHours).WithOne(e => e.Restaurant).HasForeignKey(e => e.RestaurantId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Reviews).WithOne(e => e.Restaurant).HasForeignKey(e => e.RestaurantId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Coupons).WithOne(e => e.Restaurant).HasForeignKey(e => e.RestaurantId).OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.DeliveryFee);
            if (isPostgres) entity.Property(e => e.DeliveryFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinOrderAmount);
            if (isPostgres) entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.MenuItems).WithOne(e => e.Category).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price);
            if (isPostgres) entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

            entity.Property(e => e.Images).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<string>());

            entity.HasMany(e => e.OrderItems).WithOne(e => e.MenuItem).HasForeignKey(e => e.MenuItemId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total);
            if (isPostgres) entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DeliveryFee);
            if (isPostgres) entity.Property(e => e.DeliveryFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount);
            if (isPostgres) entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasConversion<string>().HasMaxLength(50);

            entity.HasMany(e => e.Items).WithOne(e => e.Order).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.StatusHistory).WithOne(e => e.Order).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Payments).WithOne(e => e.Order).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Rider).WithMany(e => e.Orders).HasForeignKey(e => e.RiderId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Review).WithOne(e => e.Order).HasForeignKey<Review>(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Coupon).WithMany().HasForeignKey(e => e.CouponId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice);
            if (isPostgres) entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ChangedBy).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount);
            if (isPostgres) entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Method).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Reference).HasMaxLength(200);
        });

        modelBuilder.Entity<AIConversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            if (isPostgres) entity.Property(e => e.Messages).HasColumnType("jsonb");

            entity.HasOne(e => e.Order).WithOne(e => e.AiConversation)
                .HasForeignKey<AIConversation>(e => e.OrderId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Rider>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.VehicleType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Rating);
            if (isPostgres) entity.Property(e => e.Rating).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<BusinessHour>(entity =>
        {
            entity.HasKey(e => e.Id);
            if (isPostgres)
            {
                entity.Property(e => e.OpenTime).HasColumnType("time");
                entity.Property(e => e.CloseTime).HasColumnType("time");
            }
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.Property(e => e.Comment).HasMaxLength(1000);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DiscountType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.DiscountValue);
            if (isPostgres) entity.Property(e => e.DiscountValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinOrderAmount);
            if (isPostgres) entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
        });
    }
}
