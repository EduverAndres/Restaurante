using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.DTOs.Auth;
using Restaurante.Application.Features.Auth.Commands;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                Name = request.Name,
                Phone = request.Phone,
                Role = request.Role
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }
}
