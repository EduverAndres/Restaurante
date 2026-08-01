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
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalized);
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await _context.SaveChangesAsync();
    }
}
