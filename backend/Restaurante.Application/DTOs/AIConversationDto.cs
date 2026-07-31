namespace Restaurante.Application.DTOs;

public class AIConversationDto
{
    public Guid Id { get; set; }
    public string Messages { get; set; } = string.Empty;
    public string? Summary { get; set; }
}
