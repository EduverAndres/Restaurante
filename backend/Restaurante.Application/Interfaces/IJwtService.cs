using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IJwtService
{
    (string token, string refreshToken) GenerateToken(User user);
    (string accessToken, string refreshToken) RefreshToken(string refreshToken);
    Guid? ValidateRefreshToken(string refreshToken);
}
