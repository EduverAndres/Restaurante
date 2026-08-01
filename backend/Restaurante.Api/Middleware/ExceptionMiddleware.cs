using System.Net;
using FluentValidation;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;

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
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            var response = ApiResponse<object>.Fail(ex.Message);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var response = ApiResponse<object>.Fail(ex.Message);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var response = ApiResponse<object>.Fail(ex.Message);
            await context.Response.WriteAsJsonAsync(response);
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
