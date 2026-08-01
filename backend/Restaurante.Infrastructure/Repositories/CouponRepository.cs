using Microsoft.EntityFrameworkCore;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;

namespace Restaurante.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly RestauranteDbContext _context;

    public CouponRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByIdAsync(Guid id)
    {
        return await _context.Coupons.FindAsync(id);
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await GetByCodeNormalizedAsync(code.Trim().ToUpperInvariant());
    }

    public async Task<Coupon?> GetByCodeNormalizedAsync(string normalizedCode)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalizedCode);
    }

    public async Task<List<Coupon>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.Coupons
            .Where(c => c.RestaurantId == restaurantId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Coupon coupon)
    {
        await _context.Coupons.AddAsync(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Coupon coupon)
    {
        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
    }
}
