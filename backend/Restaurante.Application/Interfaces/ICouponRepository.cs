using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(Guid id);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<Coupon?> GetByCodeNormalizedAsync(string normalizedCode);
    Task<List<Coupon>> GetByRestaurantIdAsync(Guid restaurantId);
    Task AddAsync(Coupon coupon);
    Task UpdateAsync(Coupon coupon);
    Task DeleteAsync(Coupon coupon);
}
