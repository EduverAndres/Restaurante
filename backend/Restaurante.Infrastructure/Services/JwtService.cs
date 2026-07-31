using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly RefreshTokenStore _refreshTokenStore;

    public JwtService(IConfiguration config, RefreshTokenStore refreshTokenStore)
    {
        _config = config;
        _refreshTokenStore = refreshTokenStore;
    }

    public (string token, string refreshToken) GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var expiry = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");
        var refreshExpiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            },
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var refreshToken = GenerateRefreshToken();
        _refreshTokenStore.Store(refreshToken, user.Id, DateTime.UtcNow.AddDays(refreshExpiryDays));

        return (new JwtSecurityTokenHandler().WriteToken(token), refreshToken);
    }

    public (string accessToken, string refreshToken) RefreshToken(string refreshToken)
    {
        var (valid, userId) = _refreshTokenStore.Validate(refreshToken);
        if (!valid)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        _refreshTokenStore.Revoke(refreshToken);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var expiry = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");
        var refreshExpiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            },
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var newRefreshToken = GenerateRefreshToken();
        _refreshTokenStore.Store(newRefreshToken, userId, DateTime.UtcNow.AddDays(refreshExpiryDays));

        return (new JwtSecurityTokenHandler().WriteToken(token), newRefreshToken);
    }

    public Guid? ValidateRefreshToken(string refreshToken)
    {
        var (valid, userId) = _refreshTokenStore.Validate(refreshToken);
        return valid ? userId : null;
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
