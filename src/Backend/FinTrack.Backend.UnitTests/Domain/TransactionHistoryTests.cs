using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FluentAssertions;

namespace FinTrack.Backend.UnitTests.Domain;

public sealed class TransactionHistoryTests
{
    [Fact]
    public void Constructor_ShouldNormalizePayloads()
    {
        var history = new TransactionHistory(Guid.NewGuid(), Guid.NewGuid(), HistoryActionType.Created, "Created", "   ", "  next  ");

        history.Description.Should().Be("Created");
        history.PreviousValues.Should().BeNull();
        history.CurrentValues.Should().Be("next");
        history.OccurredAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PrivateConstructor_AndNavigations_ShouldBeAccessibleForMaterialization()
    {
        var history = (TransactionHistory)Activator.CreateInstance(typeof(TransactionHistory), nonPublic: true)!;

        history.User.Should().BeNull();
        history.Transaction.Should().BeNull();
    }
}
