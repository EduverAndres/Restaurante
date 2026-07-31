using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.DTOs.Auth;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Auth.Commands;

public class RegisterCommand : IRequest<ApiResponse<AuthResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "Customer";
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            return ApiResponse<AuthResponse>.Fail("Email already registered");

        var role = request.Role == "RestaurantOwner" ? UserRole.RestaurantOwner : UserRole.Customer;
        var user = new User(request.Email, request.Name, _passwordService.Hash(request.Password), role)
        {
            Phone = request.Phone
        };

        await _userRepository.AddAsync(user);

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
        }, "Registration successful");
    }
}
