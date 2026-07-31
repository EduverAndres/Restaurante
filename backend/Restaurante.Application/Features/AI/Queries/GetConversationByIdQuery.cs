using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.AI.Queries;

public class GetConversationByIdQuery : IRequest<ApiResponse<AIConversationDto>>
{
    public Guid ConversationId { get; set; }
}

public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ApiResponse<AIConversationDto>>
{
    private readonly IAIConversationRepository _repository;
    private readonly IMapper _mapper;

    public GetConversationByIdQueryHandler(IAIConversationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AIConversationDto>> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId);

        if (conversation == null)
            return ApiResponse<AIConversationDto>.Fail("Conversation not found");

        var dto = _mapper.Map<AIConversationDto>(conversation);
        return ApiResponse<AIConversationDto>.Ok(dto);
    }
}
