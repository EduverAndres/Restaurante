using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.AI.Commands;

public class SendMessageCommand : IRequest<ApiResponse<AIConversationDto>>
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ApiResponse<AIConversationDto>>
{
    private readonly IAIConversationRepository _conversationRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IAIService _aiService;
    private readonly IMapper _mapper;

    public SendMessageCommandHandler(
        IAIConversationRepository conversationRepository,
        IRestaurantRepository restaurantRepository,
        IMenuItemRepository menuItemRepository,
        IAIService aiService,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _restaurantRepository = restaurantRepository;
        _menuItemRepository = menuItemRepository;
        _aiService = aiService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AIConversationDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation is null)
            return ApiResponse<AIConversationDto>.Fail("Conversation not found");

        if (conversation.RestaurantId is not { } restaurantId)
            return ApiResponse<AIConversationDto>.Fail("Conversation has no restaurant");

        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant is null)
            return ApiResponse<AIConversationDto>.Fail("Restaurant not found");

        var menuItems = await _menuItemRepository.GetByRestaurantIdAsync(restaurantId);
        var menuContext = AIResponseValidator.BuildMenuContext(menuItems);

        var updatedMessages = $"{conversation.Messages}\nUser: {request.Content}";
        var aiResponse = await AIResponseValidator.GetValidatedResponseAsync(
            _aiService, request.Content, conversation.Messages, restaurant.Name, menuContext, menuItems);

        conversation.Messages = $"{updatedMessages}\nAI: {aiResponse}";
        conversation.Summary = aiResponse;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation);

        var dto = _mapper.Map<AIConversationDto>(conversation);
        return ApiResponse<AIConversationDto>.Ok(dto, "Message sent");
    }
}
