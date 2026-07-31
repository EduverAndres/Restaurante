using System.Net;
using FluentValidation;
using Restaurante.Application.DTOs;

namespace Restaurante.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var response = ApiResponse<object>.Fail("Validation failed", ex.Errors.Select(e => e.ErrorMessage).ToList());
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = ApiResponse<object>.Fail("An unexpected error occurred");
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
