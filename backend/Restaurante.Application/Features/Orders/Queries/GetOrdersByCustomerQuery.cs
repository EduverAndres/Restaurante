using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Orders.Queries;

public class GetOrdersByCustomerQuery : IRequest<ApiResponse<List<OrderDto>>>
{
    public Guid CustomerId { get; set; }
}

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, ApiResponse<List<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<OrderDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId);
        var dtos = _mapper.Map<List<OrderDto>>(orders);
        return ApiResponse<List<OrderDto>>.Ok(dtos);
    }
}
