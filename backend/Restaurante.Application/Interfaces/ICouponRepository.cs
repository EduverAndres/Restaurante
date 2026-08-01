using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(Guid id);
    Task<Coupon?> GetByCodeAsync(string code);
    Task UpdateAsync(Coupon coupon);
}
