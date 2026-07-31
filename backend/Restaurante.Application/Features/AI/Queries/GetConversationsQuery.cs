using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.AI.Queries;

public class GetConversationsQuery : IRequest<ApiResponse<List<AIConversationDto>>>
{
    public Guid CustomerId { get; set; }
}

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, ApiResponse<List<AIConversationDto>>>
{
    private readonly IAIConversationRepository _repository;
    private readonly IMapper _mapper;

    public GetConversationsQueryHandler(IAIConversationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<AIConversationDto>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetByCustomerIdAsync(request.CustomerId);
        var dtos = _mapper.Map<List<AIConversationDto>>(conversations);
        return ApiResponse<List<AIConversationDto>>.Ok(dtos);
    }
}
