using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Addresses.Queries;

public class GetCustomerAddressesQuery : IRequest<ApiResponse<List<CustomerAddressDto>>>
{
    public Guid UserId { get; set; }
}

public class GetCustomerAddressesQueryHandler : IRequestHandler<GetCustomerAddressesQuery, ApiResponse<List<CustomerAddressDto>>>
{
    private readonly ICustomerAddressRepository _addressRepository;
    private readonly IMapper _mapper;

    public GetCustomerAddressesQueryHandler(ICustomerAddressRepository addressRepository, IMapper mapper)
    {
        _addressRepository = addressRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<CustomerAddressDto>>> Handle(GetCustomerAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(request.UserId);
        var dtos = _mapper.Map<List<CustomerAddressDto>>(addresses);
        return ApiResponse<List<CustomerAddressDto>>.Ok(dtos);
    }
}
