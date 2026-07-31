using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Orders.Queries;

public class GetOrdersByRestaurantQuery : IRequest<ApiResponse<List<OrderDto>>>
{
    public Guid RestaurantId { get; set; }
}

public class GetOrdersByRestaurantQueryHandler : IRequestHandler<GetOrdersByRestaurantQuery, ApiResponse<List<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersByRestaurantQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<OrderDto>>> Handle(GetOrdersByRestaurantQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByRestaurantIdAsync(request.RestaurantId);
        var dtos = _mapper.Map<List<OrderDto>>(orders);
        return ApiResponse<List<OrderDto>>.Ok(dtos);
    }
}
