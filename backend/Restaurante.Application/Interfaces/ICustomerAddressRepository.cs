using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface ICustomerAddressRepository
{
    Task<List<CustomerAddress>> GetByUserIdAsync(Guid userId);
    Task<CustomerAddress?> GetByIdAsync(Guid id);
    Task AddAsync(CustomerAddress address);
    Task UpdateAsync(CustomerAddress address);
    Task DeleteAsync(CustomerAddress address);
}
