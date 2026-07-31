using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.DTOs.Auth;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<ApiResponse<AuthResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(IJwtService jwtService, IUserRepository userRepository, IMapper mapper)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.ValidateRefreshToken(request.RefreshToken);
        if (userId is null)
            return ApiResponse<AuthResponse>.Fail("Invalid or expired refresh token");

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user is null)
            return ApiResponse<AuthResponse>.Fail("User not found");

        var (newToken, newRefreshToken) = _jwtService.RefreshToken(request.RefreshToken);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            User = _mapper.Map<UserDto>(user)
        });
    }
}
