using FinTrack.Application.Common.Models;
using FinTrack.Domain.Enums;
using FluentAssertions;

namespace FinTrack.Backend.UnitTests.Application;

public sealed class ModelRecordTests
{
    [Fact]
    public void TransactionDto_ShouldExposeAllValues()
    {
        var createdAt = DateTime.UtcNow;
        var updatedAt = createdAt.AddMinutes(1);
        var dto = new TransactionDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Salary", 100m, createdAt, TransactionType.Income, "Salary payment", false, createdAt, updatedAt);

        dto.UserId.Should().NotBeEmpty();
        dto.CreatedAtUtc.Should().Be(createdAt);
        dto.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void TransactionHistoryDto_ShouldExposeAllValues()
    {
        var occurredAt = DateTime.UtcNow;
        var dto = new TransactionHistoryDto(Guid.NewGuid(), Guid.NewGuid(), HistoryActionType.Updated, "Updated", "before", "after", occurredAt);

        dto.Id.Should().NotBeEmpty();
        dto.TransactionId.Should().NotBeEmpty();
        dto.Action.Should().Be(HistoryActionType.Updated);
        dto.Description.Should().Be("Updated");
        dto.PreviousValues.Should().Be("before");
        dto.CurrentValues.Should().Be("after");
        dto.OccurredAtUtc.Should().Be(occurredAt);
    }
}
