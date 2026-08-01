using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Helpers;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Orders.Commands;

public class AssignRiderCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid OrderId { get; set; }
    public Guid? RiderId { get; set; }
    public string ChangedBy { get; set; } = "System";
}

public class AssignRiderCommandHandler : IRequestHandler<AssignRiderCommand, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRiderRepository _riderRepository;
    private readonly IOrderNotifier _notifier;
    private readonly IMapper _mapper;

    public AssignRiderCommandHandler(IOrderRepository orderRepository, IRiderRepository riderRepository, IOrderNotifier notifier, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _riderRepository = riderRepository;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ApiResponse<OrderDto>> Handle(AssignRiderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
            throw new NotFoundException("Order not found");

        if (order.Status != OrderStatus.Ready && order.Status != OrderStatus.AssignedToRider)
            return ApiResponse<OrderDto>.Fail("Order must be ready to assign a rider");

        Rider? rider;
        if (request.RiderId.HasValue)
        {
            rider = await _riderRepository.GetByIdAsync(request.RiderId.Value);
            if (rider is null)
                throw new NotFoundException("Rider not found");

            if (rider.Status != RiderStatus.Available && order.RiderId != rider.Id)
                return ApiResponse<OrderDto>.Fail("Rider is not available");
        }
        else
        {
            var candidates = await _riderRepository.GetAvailableAsync();
            rider = SelectNearestRider(candidates, order.Restaurant);
            if (rider is null)
                return ApiResponse<OrderDto>.Fail("No available riders");
        }

        var previousRiderId = order.RiderId;

        var fromStatus = order.Status;
        order.RiderId = rider.Id;
        order.AssignedAt = DateTime.UtcNow;
        order.Status = OrderStatus.AssignedToRider;
        order.UpdatedAt = DateTime.UtcNow;
        order.StatusHistory.Add(new OrderStatusHistory(order.Id, fromStatus, OrderStatus.AssignedToRider, request.ChangedBy));

        rider.Status = RiderStatus.Busy;
        rider.UpdatedAt = DateTime.UtcNow;

        await _riderRepository.UpdateAsync(rider);
        await _orderRepository.UpdateAsync(order);

        if (previousRiderId.HasValue && previousRiderId.Value != rider.Id)
        {
            var previousRider = await _riderRepository.GetByIdAsync(previousRiderId.Value);
            if (previousRider is not null && previousRider.Status == RiderStatus.Busy)
            {
                previousRider.Status = RiderStatus.Available;
                previousRider.UpdatedAt = DateTime.UtcNow;
                await _riderRepository.UpdateAsync(previousRider);
            }
        }

        var dto = _mapper.Map<OrderDto>(order);

        await _notifier.NotifyOrderUpdated(order.RestaurantId, dto);
        await _notifier.NotifyOrderStatusChanged(order.Id, dto);

        return ApiResponse<OrderDto>.Ok(dto, "Rider assigned to order");
    }

    private static Rider? SelectNearestRider(List<Rider> riders, Restaurant restaurant)
    {
        var withLocation = riders.Where(r => r.Latitude.HasValue && r.Longitude.HasValue).ToList();
        if (withLocation.Count == 0)
            return null;

        if (restaurant.Latitude.HasValue && restaurant.Longitude.HasValue)
        {
            var withDistance = withLocation
                .Select(r => (Rider: r,
                    Distance: GeoHelper.DistanceKm(
                        restaurant.Latitude.Value, restaurant.Longitude.Value,
                        r.Latitude!.Value, r.Longitude!.Value)))
                .ToList();

            if (restaurant.RadiusKm.HasValue)
                withDistance = withDistance.Where(x => x.Distance <= restaurant.RadiusKm.Value).ToList();

            return withDistance.OrderBy(x => x.Distance).Select(x => x.Rider).FirstOrDefault();
        }

        return withLocation.OrderByDescending(r => r.Rating).ThenBy(r => r.CreatedAt).FirstOrDefault();
    }
}
