using System.Text.Json;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Models;
using FinTrack.WebAPI.Middlewares;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinTrack.Backend.UnitTests.WebAPI;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldHandleValidationException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new ValidationException(new[] { new ValidationFailure("Field", "Field is invalid.") }), NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var payload = await ReadResponseAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        payload.message.Should().Be("Falha na validação.");
        payload.errors.Should().Contain("Field is invalid.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleNotFoundException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new NotFoundException("missing"), NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var payload = await ReadResponseAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        payload.message.Should().Be("missing");
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleUnauthorizedException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new UnauthorizedException("unauthorized"), NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var payload = await ReadResponseAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        payload.message.Should().Be("unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleGenericException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("boom"), NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var payload = await ReadResponseAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        payload.message.Should().Be("Ocorreu um erro inesperado.");
    }

    private static async Task<(string message, string[] errors)> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;
        return (
            root.GetProperty("message").GetString()!,
            root.GetProperty("errors").EnumerateArray().Select(error => error.GetString()!).ToArray());
    }
}
