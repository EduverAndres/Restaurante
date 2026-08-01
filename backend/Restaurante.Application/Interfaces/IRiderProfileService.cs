using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IRiderProfileService
{
    Task<Rider> EnsureRiderAsync(Guid userId);
}
