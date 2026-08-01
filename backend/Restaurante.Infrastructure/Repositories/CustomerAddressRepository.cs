using Microsoft.EntityFrameworkCore;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;

namespace Restaurante.Infrastructure.Repositories;

public class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly RestauranteDbContext _context;

    public CustomerAddressRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerAddress>> GetByUserIdAsync(Guid userId)
    {
        return await _context.CustomerAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<CustomerAddress?> GetByIdAsync(Guid id)
    {
        return await _context.CustomerAddresses.FindAsync(id);
    }

    public async Task AddAsync(CustomerAddress address)
    {
        await _context.CustomerAddresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CustomerAddress address)
    {
        _context.CustomerAddresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(CustomerAddress address)
    {
        _context.CustomerAddresses.Remove(address);
        await _context.SaveChangesAsync();
    }
}
