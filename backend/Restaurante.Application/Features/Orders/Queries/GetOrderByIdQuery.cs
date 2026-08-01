using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Orders.Queries;

public class GetOrderByIdQuery : IRequest<ApiResponse<OrderDto>>
{
    public Guid OrderId { get; set; }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
            return ApiResponse<OrderDto>.Fail("Order not found");

        var dto = _mapper.Map<OrderDto>(order);
        return ApiResponse<OrderDto>.Ok(dto);
    }
}