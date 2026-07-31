using System.Collections.Concurrent;

namespace Restaurante.Infrastructure.Services;

public class RefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenEntry> _store = new();

    public void Store(string token, Guid userId, DateTime expiresAt)
    {
        _store[token] = new RefreshTokenEntry(userId, expiresAt);
    }

    public (bool valid, Guid userId) Validate(string token)
    {
        if (!_store.TryGetValue(token, out var entry))
            return (false, Guid.Empty);

        if (entry.Revoked || DateTime.UtcNow > entry.ExpiresAt)
        {
            _store.TryRemove(token, out _);
            return (false, Guid.Empty);
        }

        return (true, entry.UserId);
    }

    public void Revoke(string token)
    {
        if (_store.TryGetValue(token, out var entry))
        {
            entry.Revoked = true;
        }
    }
}

public class RefreshTokenEntry
{
    public Guid UserId { get; }
    public DateTime ExpiresAt { get; }
    public bool Revoked { get; set; }

    public RefreshTokenEntry(Guid userId, DateTime expiresAt)
    {
        UserId = userId;
        ExpiresAt = expiresAt;
    }
}
