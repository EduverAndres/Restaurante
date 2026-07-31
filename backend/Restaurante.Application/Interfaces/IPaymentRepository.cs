using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<List<Payment>> GetByOrderIdAsync(Guid orderId);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}
