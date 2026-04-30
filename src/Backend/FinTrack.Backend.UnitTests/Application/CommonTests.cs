using FinTrack.Application;
using FinTrack.Application.Common.Behaviors;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using FinTrack.Application.Common.Abstractions;
using FinTrack.Backend.UnitTests.Testing;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Backend.UnitTests.Application;

public sealed class CommonTests
{
    [Fact]
    public void ApiResponse_OkAndFail_ShouldProduceExpectedPayload()
    {
        var ok = ApiResponse<string>.Ok("data", "done");
        var fail = ApiResponse<string>.Fail("failed", "error-1", "error-2");

        ok.Success.Should().BeTrue();
        ok.Data.Should().Be("data");
        ok.Errors.Should().BeEmpty();

        fail.Success.Should().BeFalse();
        fail.Data.Should().BeNull();
        fail.Errors.Should().Contain(new[] { "error-1", "error-2" });
    }

    [Fact]
    public void CurrentUserAccessor_ShouldReturnUserIdOrThrow()
    {
        var userId = Guid.NewGuid();

        CurrentUserAccessor.GetRequiredUserId(new TestCurrentUserService { UserId = userId }).Should().Be(userId);

        var action = () => CurrentUserAccessor.GetRequiredUserId(new TestCurrentUserService());

        action.Should().Throw<UnauthorizedException>()
            .WithMessage("The X-User-Id header is required.");
    }

    [Fact]
    public async Task UserGuard_ShouldValidateExistingUser()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, _, _) = TestDbContextFactory.SeedUserWithCategories(context);

        await UserGuard.EnsureExistsAsync(context, user.Id, CancellationToken.None);

        var action = async () => await UserGuard.EnsureExistsAsync(context, Guid.NewGuid(), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User '*' was not found.");
    }

    [Fact]
    public async Task ValidationBehavior_ShouldBypassWhenThereAreNoValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());

        var result = await behavior.Handle(new TestRequest("ok"), () => Task.FromResult("next"), CancellationToken.None);

        result.Should().Be("next");
    }

    [Fact]
    public async Task ValidationBehavior_ShouldThrowWhenValidationFails()
    {
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var action = async () => await behavior.Handle(new TestRequest(string.Empty), () => Task.FromResult("next"), CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ValidationBehavior_ShouldContinueWhenValidationSucceeds()
    {
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var result = await behavior.Handle(new TestRequest("valid"), () => Task.FromResult("next"), CancellationToken.None);

        result.Should().Be("next");
    }

    [Fact]
    public void AddApplication_ShouldRegisterMediatorPipelineAndValidators()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(MediatR.IPipelineBehavior<,>));

        using var provider = services.BuildServiceProvider();
        provider.GetServices<IValidator<FinTrack.Application.Features.Transactions.Commands.CreateTransactionCommand>>()
            .Should()
            .NotBeEmpty();
    }

    private sealed record TestRequest(string Value);

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(request => request.Value).NotEmpty();
        }
    }
}
