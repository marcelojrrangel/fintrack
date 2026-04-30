using FinTrack.Application;
using FinTrack.Application.Common.Abstractions;
using FinTrack.Infrastructure;
using FinTrack.WebAPI.Middlewares;
using FinTrack.WebAPI.Services;
using FinTrack.WebAPI.Swagger;
using FluentValidation.AspNetCore;
using System.Text.Json.Serialization;
using Serilog;
using Serilog.Formatting.Compact;

// Configurar Serilog ANTES de criar o builder
// Estou colocando este exemplo em arquivo somente para ilustração e a importância de logs em APIs.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "FinTrack.WebAPI")
    .WriteTo.Console()
    .WriteTo.File(
        new CompactJsonFormatter(),
        path: "logs/fintrack-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicação FinTrack WebAPI");

    var builder = WebApplication.CreateBuilder(args);

    // Adicionar Serilog ao pipeline de logging do ASP.NET Core
    builder.Host.UseSerilog();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.OperationFilter<UserHeaderOperationFilter>();
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await initializer.InitializeAsync();
        Log.Information("Banco de dados inicializado com sucesso");
    }

    // Middleware de correlação e performance logging
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<PerformanceLoggingMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Adicionar request logging do Serilog
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());

            if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var userId))
            {
                diagnosticContext.Set("UserId", userId.ToString());
            }
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("FinTrack WebAPI iniciada com sucesso");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
}
