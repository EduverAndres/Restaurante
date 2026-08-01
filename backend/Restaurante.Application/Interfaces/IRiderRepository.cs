using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IRiderRepository
{
    Task<Rider?> GetByIdAsync(Guid id);
    Task<Rider?> GetByUserIdAsync(Guid userId);
    Task<List<Rider>> GetAvailableAsync();
    Task AddAsync(Rider rider);
    Task UpdateAsync(Rider rider);
}
