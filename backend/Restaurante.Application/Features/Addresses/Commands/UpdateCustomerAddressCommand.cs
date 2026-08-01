using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Addresses.Commands;

public class UpdateCustomerAddressCommand : IRequest<ApiResponse<CustomerAddressDto>>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateCustomerAddressCommandHandler : IRequestHandler<UpdateCustomerAddressCommand, ApiResponse<CustomerAddressDto>>
{
    private readonly ICustomerAddressRepository _addressRepository;
    private readonly IMapper _mapper;

    public UpdateCustomerAddressCommandHandler(ICustomerAddressRepository addressRepository, IMapper mapper)
    {
        _addressRepository = addressRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CustomerAddressDto>> Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.Id);
        if (address is null || address.UserId != request.UserId)
            throw new NotFoundException("Address not found");

        if (request.IsDefault && !address.IsDefault)
        {
            var existing = await _addressRepository.GetByUserIdAsync(request.UserId);
            foreach (var addr in existing.Where(a => a.Id != address.Id && a.IsDefault))
            {
                addr.IsDefault = false;
                await _addressRepository.UpdateAsync(addr);
            }
        }

        address.Label = request.Label;
        address.Address = request.Address;
        address.Latitude = request.Latitude;
        address.Longitude = request.Longitude;
        address.IsDefault = request.IsDefault;
        address.UpdatedAt = DateTime.UtcNow;

        await _addressRepository.UpdateAsync(address);

        var dto = _mapper.Map<CustomerAddressDto>(address);
        return ApiResponse<CustomerAddressDto>.Ok(dto, "Address updated");
    }
}
