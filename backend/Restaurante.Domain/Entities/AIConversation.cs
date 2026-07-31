using Restaurante.Domain.Common;

namespace Restaurante.Domain.Entities;

public class AIConversation : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public Guid? RestaurantId { get; set; }
    public string Messages { get; set; }
    public string? Summary { get; set; }

    public AIConversation(Guid customerId, string messages)
    {
        CustomerId = customerId;
        Messages = messages;
    }
}
