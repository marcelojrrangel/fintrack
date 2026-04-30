using System.Diagnostics;

namespace FinTrack.WebAPI.Middlewares;

/// <summary>
/// Middleware que monitora a performance das requisições e loga avisos para requisições lentas
/// </summary>
public class PerformanceLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;
    private const int SlowRequestThresholdMs = 500;

    public PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > SlowRequestThresholdMs)
            {
                var requestPath = $"{context.Request.Method} {context.Request.Path}";

                _logger.LogWarning(
                    "Requisição lenta detectada: {RequestPath} levou {ElapsedMilliseconds}ms (limite: {ThresholdMs}ms). StatusCode: {StatusCode}",
                    requestPath,
                    elapsedMilliseconds,
                    SlowRequestThresholdMs,
                    context.Response.StatusCode);
            }
            else
            {
                _logger.LogInformation(
                    "Requisição {Method} {Path} concluída em {ElapsedMilliseconds}ms com status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMilliseconds,
                    context.Response.StatusCode);
            }
        }
    }
}
