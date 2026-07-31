using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.DTOs.Auth;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<ApiResponse<AuthResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid email or password");

        var (token, refreshToken) = _jwtService.GenerateToken(user);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Avatar = user.Avatar,
                Phone = user.Phone
            }
        });
    }
}
