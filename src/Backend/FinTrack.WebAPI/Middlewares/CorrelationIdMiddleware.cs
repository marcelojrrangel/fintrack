using Serilog.Context;

namespace FinTrack.WebAPI.Middlewares;

/// <summary>
/// Middleware que adiciona um CorrelationId a cada requisição para rastreamento em logs
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obter ou gerar um CorrelationId
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();

        // Adicionar ao contexto de resposta
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        // Adicionar ao contexto de log do Serilog
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Requisição iniciada com CorrelationId: {CorrelationId}", correlationId);

            await _next(context);

            _logger.LogDebug("Requisição concluída com CorrelationId: {CorrelationId}", correlationId);
        }
    }
}
