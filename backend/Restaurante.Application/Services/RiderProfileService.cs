using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Services;

public class RiderProfileService : IRiderProfileService
{
    private readonly IRiderRepository _riderRepository;

    public RiderProfileService(IRiderRepository riderRepository)
    {
        _riderRepository = riderRepository;
    }

    public async Task<Rider> EnsureRiderAsync(Guid userId)
    {
        var rider = await _riderRepository.GetByUserIdAsync(userId);
        if (rider is not null)
            return rider;

        rider = new Rider
        {
            UserId = userId,
            VehicleType = VehicleType.Bike,
            Status = RiderStatus.Offline,
            Rating = 0,
            RatingsCount = 0
        };

        await _riderRepository.AddAsync(rider);
        return rider;
    }
}
