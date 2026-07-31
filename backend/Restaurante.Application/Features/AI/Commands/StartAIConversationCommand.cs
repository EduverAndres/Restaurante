using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.AI.Commands;

public class StartAIConversationCommand : IRequest<ApiResponse<AIConversationDto>>
{
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string InitialMessage { get; set; } = string.Empty;
}

public class StartAIConversationCommandHandler : IRequestHandler<StartAIConversationCommand, ApiResponse<AIConversationDto>>
{
    private readonly IAIConversationRepository _conversationRepository;
    private readonly IAIService _aiService;
    private readonly IMapper _mapper;

    public StartAIConversationCommandHandler(
        IAIConversationRepository conversationRepository,
        IAIService aiService,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _aiService = aiService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AIConversationDto>> Handle(StartAIConversationCommand request, CancellationToken cancellationToken)
    {
        var summary = await _aiService.ProcessOrderConversationAsync(request.InitialMessage);
        var conversation = new AIConversation(request.CustomerId, request.InitialMessage)
        {
            Summary = summary,
            RestaurantId = request.RestaurantId
        };

        await _conversationRepository.AddAsync(conversation);

        var dto = _mapper.Map<AIConversationDto>(conversation);
        return ApiResponse<AIConversationDto>.Ok(dto, "Conversation started");
    }
}
