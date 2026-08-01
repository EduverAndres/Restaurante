using Restaurante.Domain.Common;
using Restaurante.Domain.Enums;

namespace Restaurante.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }

    public List<Restaurant> Restaurants { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<AIConversation> AiConversations { get; set; } = new();
    public List<Rider> Riders { get; set; } = new();
    public List<CustomerAddress> CustomerAddresses { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();

    public User(string email, string name, string passwordHash, UserRole role)
    {
        Email = email;
        Name = name;
        PasswordHash = passwordHash;
        Role = role;
    }
}
