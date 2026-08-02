using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IBusinessHourRepository
{
    Task ReplaceAsync(Guid restaurantId, IEnumerable<BusinessHour> hours);
}
