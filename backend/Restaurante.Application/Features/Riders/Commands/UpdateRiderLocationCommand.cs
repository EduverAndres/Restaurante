using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Riders.Commands;

public class UpdateRiderLocationCommand : IRequest<ApiResponse<RiderDto>>
{
    public Guid UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class UpdateRiderLocationCommandHandler : IRequestHandler<UpdateRiderLocationCommand, ApiResponse<RiderDto>>
{
    private readonly IRiderProfileService _riderProfileService;
    private readonly IRiderRepository _riderRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderNotifier _notifier;
    private readonly IMapper _mapper;

    public UpdateRiderLocationCommandHandler(
        IRiderProfileService riderProfileService,
        IRiderRepository riderRepository,
        IOrderRepository orderRepository,
        IOrderNotifier notifier,
        IMapper mapper)
    {
        _riderProfileService = riderProfileService;
        _riderRepository = riderRepository;
        _orderRepository = orderRepository;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RiderDto>> Handle(UpdateRiderLocationCommand request, CancellationToken cancellationToken)
    {
        var rider = await _riderProfileService.EnsureRiderAsync(request.UserId);

        rider.Latitude = request.Latitude;
        rider.Longitude = request.Longitude;
        rider.LastLocationAt = DateTime.UtcNow;
        rider.UpdatedAt = DateTime.UtcNow;

        await _riderRepository.UpdateAsync(rider);

        var activeOrders = await _orderRepository.GetByRiderIdAsync(rider.Id);
        foreach (var order in activeOrders.Where(o =>
                     o.Status == OrderStatus.AssignedToRider || o.Status == OrderStatus.OutForDelivery))
        {
            await _notifier.NotifyRiderLocationUpdatedAsync(order.Id, request.Latitude, request.Longitude);
        }

        var dto = _mapper.Map<RiderDto>(rider);
        return ApiResponse<RiderDto>.Ok(dto, "Rider location updated");
    }
}
