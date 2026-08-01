using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Riders.Queries;

public class GetRiderOrdersQuery : IRequest<ApiResponse<List<OrderDto>>>
{
    public Guid UserId { get; set; }
}

public class GetRiderOrdersQueryHandler : IRequestHandler<GetRiderOrdersQuery, ApiResponse<List<OrderDto>>>
{
    private readonly IRiderRepository _riderRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetRiderOrdersQueryHandler(IRiderRepository riderRepository, IOrderRepository orderRepository, IMapper mapper)
    {
        _riderRepository = riderRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<OrderDto>>> Handle(GetRiderOrdersQuery request, CancellationToken cancellationToken)
    {
        var rider = await _riderRepository.GetByUserIdAsync(request.UserId);
        if (rider is null)
            return ApiResponse<List<OrderDto>>.Ok(new List<OrderDto>());

        var orders = await _orderRepository.GetByRiderIdAsync(rider.Id);
        var dtos = _mapper.Map<List<OrderDto>>(orders);
        return ApiResponse<List<OrderDto>>.Ok(dtos);
    }
}
