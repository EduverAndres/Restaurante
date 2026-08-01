using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.AI.Commands;
using Restaurante.Application.Features.AI.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("conversation/start")]
    public async Task<ActionResult<ApiResponse<AIConversationDto>>> StartConversation([FromBody] StartConversationRequest request)
    {
        try
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new StartAIConversationCommand
            {
                CustomerId = customerId,
                RestaurantId = request.RestaurantId,
                InitialMessage = request.InitialMessage ?? "Hola, quiero ordenar"
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AIConversationDto>.Fail(ex.Message));
        }
    }

    [HttpPost("conversation/{id}/message")]
    public async Task<ActionResult<ApiResponse<AIConversationDto>>> SendMessage(Guid id, [FromBody] SendMessageRequest request)
    {
        try
        {
            var command = new SendMessageCommand
            {
                ConversationId = id,
                Content = request.Content
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AIConversationDto>.Fail(ex.Message));
        }
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<List<AIConversationDto>>>> GetAll()
    {
        try
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetConversationsQuery { CustomerId = customerId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<AIConversationDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("conversation/{id}")]
    public async Task<ActionResult<ApiResponse<AIConversationDto>>> GetById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetConversationByIdQuery { ConversationId = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AIConversationDto>.Fail(ex.Message));
        }
    }
}

public class StartConversationRequest
{
    public string? InitialMessage { get; set; }
    public Guid RestaurantId { get; set; }
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
}
