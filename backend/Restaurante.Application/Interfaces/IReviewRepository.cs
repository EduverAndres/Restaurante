using Restaurante.Application.DTOs;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IReviewRepository
{
    Task<List<Review>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<Review?> GetByOrderIdAsync(Guid orderId);
    Task AddAsync(Review review);
    Task<Dictionary<Guid, RatingSummary>> GetRatingSummaryAsync(IReadOnlyCollection<Guid> restaurantIds);
}
