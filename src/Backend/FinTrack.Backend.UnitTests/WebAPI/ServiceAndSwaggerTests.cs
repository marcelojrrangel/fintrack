using System.Reflection;
using FinTrack.Application.Common.Models;
using FinTrack.WebAPI.Controllers;
using FinTrack.WebAPI.Services;
using FinTrack.WebAPI.Swagger;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using NSubstitute;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinTrack.Backend.UnitTests.WebAPI;

public sealed class ServiceAndSwaggerTests
{
    [Fact]
    public void HttpCurrentUserService_ShouldReadGuidHeader()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var userId = Guid.NewGuid();
        context.Request.Headers["X-User-Id"] = userId.ToString();
        httpContextAccessor.HttpContext.Returns(context);

        var service = new HttpCurrentUserService(httpContextAccessor);

        service.UserId.Should().Be(userId);
    }

    [Fact]
    public void HttpCurrentUserService_ShouldReturnNullForMissingOrInvalidHeader()
    {
        var accessorWithoutContext = Substitute.For<IHttpContextAccessor>();
        var invalidHeaderAccessor = Substitute.For<IHttpContextAccessor>();
        invalidHeaderAccessor.HttpContext.Returns(new DefaultHttpContext());
        invalidHeaderAccessor.HttpContext!.Request.Headers["X-User-Id"] = "invalid";

        new HttpCurrentUserService(accessorWithoutContext).UserId.Should().BeNull();
        new HttpCurrentUserService(invalidHeaderAccessor).UserId.Should().BeNull();
    }

    [Fact]
    public void UserHeaderOperationFilter_ShouldSkipHealthEndpoints()
    {
        var operation = new OpenApiOperation();
        var context = CreateContext("api/health", typeof(HealthController).GetMethod(nameof(HealthController.Get))!);

        new UserHeaderOperationFilter().Apply(operation, context);

        operation.Parameters.Should().BeNullOrEmpty();
    }

    [Fact]
    public void UserHeaderOperationFilter_ShouldAppendRequiredHeader()
    {
        var operation = new OpenApiOperation();
        var context = CreateContext("api/dashboard", typeof(DashboardController).GetMethod(nameof(DashboardController.Get))!);

        new UserHeaderOperationFilter().Apply(operation, context);

        operation.Parameters.Should().ContainSingle(parameter => parameter.Name == "X-User-Id" && parameter.Required);
    }

    private static OperationFilterContext CreateContext(string relativePath, MethodInfo methodInfo)
    {
        return new OperationFilterContext(
            new ApiDescription { RelativePath = relativePath },
            Substitute.For<ISchemaGenerator>(),
            new SchemaRepository(),
            methodInfo);
    }
}
