using FinTrack.Domain.Common;
using FluentAssertions;

namespace FinTrack.Backend.UnitTests.Domain;

public sealed class GuardTests
{
    [Fact]
    public void AgainstNullOrWhiteSpace_ShouldTrimAndReturnValue()
    {
        var result = Guard.AgainstNullOrWhiteSpace("  valid  ", "name", 10);

        result.Should().Be("valid");
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_ShouldThrowWhenValueIsBlank()
    {
        var action = () => Guard.AgainstNullOrWhiteSpace("   ", "name", 10);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_ShouldThrowWhenValueExceedsLength()
    {
        var action = () => Guard.AgainstNullOrWhiteSpace("toolong", "name", 3);

        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("name");
    }

    [Fact]
    public void AgainstNegativeOrZero_ShouldValidateValue()
    {
        Guard.AgainstNegativeOrZero(10m, "amount").Should().Be(10m);

        Action zero = () => Guard.AgainstNegativeOrZero(0m, "amount");
        Action negative = () => Guard.AgainstNegativeOrZero(-1m, "amount");

        zero.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("amount");
        negative.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("amount");
    }
}
