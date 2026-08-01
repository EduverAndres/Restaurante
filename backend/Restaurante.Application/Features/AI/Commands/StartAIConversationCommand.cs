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
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IAIService _aiService;
    private readonly IMapper _mapper;

    public StartAIConversationCommandHandler(
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

    public async Task<ApiResponse<AIConversationDto>> Handle(StartAIConversationCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            return ApiResponse<AIConversationDto>.Fail("Restaurant not found");

        var menuItems = await _menuItemRepository.GetByRestaurantIdAsync(request.RestaurantId);
        var menuContext = AIResponseValidator.BuildMenuContext(menuItems);

        var aiResponse = await AIResponseValidator.GetValidatedResponseAsync(
            _aiService, request.InitialMessage, null, restaurant.Name, menuContext, menuItems);

        var conversation = new AIConversation(request.CustomerId, "User: " + request.InitialMessage)
        {
            Summary = aiResponse,
            RestaurantId = request.RestaurantId
        };

        await _conversationRepository.AddAsync(conversation);

        var dto = _mapper.Map<AIConversationDto>(conversation);
        return ApiResponse<AIConversationDto>.Ok(dto, "Conversation started");
    }
}
