using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.AI.Commands;

public class SendMessageCommand : IRequest<ApiResponse<AIConversationDto>>
{
    public Guid ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ApiResponse<AIConversationDto>>
{
    private readonly IAIConversationRepository _conversationRepository;
    private readonly IAIService _aiService;
    private readonly IMapper _mapper;

    public SendMessageCommandHandler(
        IAIConversationRepository conversationRepository,
        IAIService aiService,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _aiService = aiService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AIConversationDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation is null)
            return ApiResponse<AIConversationDto>.Fail("Conversation not found");

        var updatedMessages = $"{conversation.Messages}\nUser: {request.Message}";
        var aiResponse = await _aiService.ProcessOrderConversationAsync(request.Message, conversation.Messages);

        conversation.Messages = $"{updatedMessages}\nAI: {aiResponse}";
        conversation.Summary = aiResponse;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation);

        var dto = _mapper.Map<AIConversationDto>(conversation);
        return ApiResponse<AIConversationDto>.Ok(dto, "Message sent");
    }
}
