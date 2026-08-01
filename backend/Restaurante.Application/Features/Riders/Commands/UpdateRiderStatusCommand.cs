using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Riders.Commands;

public class UpdateRiderStatusCommand : IRequest<ApiResponse<RiderDto>>
{
    public Guid UserId { get; set; }
    public RiderStatus Status { get; set; }
}

public class UpdateRiderStatusCommandHandler : IRequestHandler<UpdateRiderStatusCommand, ApiResponse<RiderDto>>
{
    private readonly IRiderProfileService _riderProfileService;
    private readonly IRiderRepository _riderRepository;
    private readonly IMapper _mapper;

    public UpdateRiderStatusCommandHandler(IRiderProfileService riderProfileService, IRiderRepository riderRepository, IMapper mapper)
    {
        _riderProfileService = riderProfileService;
        _riderRepository = riderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RiderDto>> Handle(UpdateRiderStatusCommand request, CancellationToken cancellationToken)
    {
        var rider = await _riderProfileService.EnsureRiderAsync(request.UserId);

        rider.Status = request.Status;
        rider.UpdatedAt = DateTime.UtcNow;

        await _riderRepository.UpdateAsync(rider);

        var dto = _mapper.Map<RiderDto>(rider);
        return ApiResponse<RiderDto>.Ok(dto, $"Rider status updated to {request.Status}");
    }
}
