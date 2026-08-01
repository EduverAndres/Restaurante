using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.Addresses.Commands;

public class CreateCustomerAddressCommand : IRequest<ApiResponse<CustomerAddressDto>>
{
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateCustomerAddressCommandHandler : IRequestHandler<CreateCustomerAddressCommand, ApiResponse<CustomerAddressDto>>
{
    private readonly ICustomerAddressRepository _addressRepository;
    private readonly IMapper _mapper;

    public CreateCustomerAddressCommandHandler(ICustomerAddressRepository addressRepository, IMapper mapper)
    {
        _addressRepository = addressRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CustomerAddressDto>> Handle(CreateCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var existing = await _addressRepository.GetByUserIdAsync(request.UserId);

        var isDefault = request.IsDefault || existing.Count == 0;
        if (isDefault && existing.Count > 0)
        {
            foreach (var addr in existing.Where(a => a.IsDefault))
            {
                addr.IsDefault = false;
                await _addressRepository.UpdateAsync(addr);
            }
        }

        var address = new CustomerAddress
        {
            UserId = request.UserId,
            Label = request.Label,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = isDefault
        };

        await _addressRepository.AddAsync(address);

        var dto = _mapper.Map<CustomerAddressDto>(address);
        return ApiResponse<CustomerAddressDto>.Ok(dto, "Address created");
    }
}
