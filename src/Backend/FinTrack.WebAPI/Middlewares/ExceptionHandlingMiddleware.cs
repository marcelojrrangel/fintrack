using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Models;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace FinTrack.WebAPI.Middlewares;

/// <summary>
/// Middleware global para captura e logging de exceções não tratadas
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Falha na validação.",
                validationException.Errors.Select(error => error.ErrorMessage).Distinct().ToArray()),

            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                notFoundException.Message,
                Array.Empty<string>()),

            UnauthorizedException unauthorizedException => (
                HttpStatusCode.Unauthorized,
                unauthorizedException.Message,
                Array.Empty<string>()),

            _ => (
                HttpStatusCode.InternalServerError,
                "Ocorreu um erro inesperado.",
                Array.Empty<string>())
        };

        // Logging detalhado baseado no tipo de exceção
        LogException(context, exception, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(
            ApiResponse<object>.Fail(message, errors),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await context.Response.WriteAsync(payload);
    }

    private void LogException(HttpContext context, Exception exception, HttpStatusCode statusCode)
    {
        var requestPath = $"{context.Request.Method} {context.Request.Path}";
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault() ?? "Anonymous";

        switch (exception)
        {
            case ValidationException validationException:
                // Validation errors são esperados - LogWarning
                _logger.LogWarning(
                    validationException,
                    "Falha na validação para {RequestPath}. UserId: {UserId}. Erros: {ValidationErrors}",
                    requestPath,
                    userId,
                    string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage)));
                break;

            case NotFoundException notFoundException:
                // NotFound é esperado em alguns cenários - LogWarning
                _logger.LogWarning(
                    notFoundException,
                    "Recurso não encontrado para {RequestPath}. UserId: {UserId}. Mensagem: {Message}",
                    requestPath,
                    userId,
                    notFoundException.Message);
                break;

            case UnauthorizedException unauthorizedException:
                // Unauthorized - possível tentativa de acesso indevido - LogWarning
                _logger.LogWarning(
                    unauthorizedException,
                    "Tentativa de acesso não autorizado para {RequestPath}. UserId: {UserId}. Mensagem: {Message}",
                    requestPath,
                    userId,
                    unauthorizedException.Message);
                break;

            default:
                // Exceções inesperadas são CRÍTICAS - LogError com stack trace completo
                _logger.LogError(
                    exception,
                    "CRÍTICO: Exceção não tratada ocorreu para {RequestPath}. " +
                    "UserId: {UserId}. StatusCode: {StatusCode}. " +
                    "ExceptionType: {ExceptionType}. Mensagem: {Message}. StackTrace: {StackTrace}",
                    requestPath,
                    userId,
                    (int)statusCode,
                    exception.GetType().Name,
                    exception.Message,
                    exception.StackTrace);
                break;
        }
    }
}
