using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Addresses.Commands;

public class DeleteCustomerAddressCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class DeleteCustomerAddressCommandHandler : IRequestHandler<DeleteCustomerAddressCommand, ApiResponse<bool>>
{
    private readonly ICustomerAddressRepository _addressRepository;

    public DeleteCustomerAddressCommandHandler(ICustomerAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.Id);
        if (address is null || address.UserId != request.UserId)
            throw new NotFoundException("Address not found");

        await _addressRepository.DeleteAsync(address);
        return ApiResponse<bool>.Ok(true, "Address deleted");
    }
}
