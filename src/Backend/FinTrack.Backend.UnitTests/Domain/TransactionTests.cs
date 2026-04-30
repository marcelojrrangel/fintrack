using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FluentAssertions;

namespace FinTrack.Backend.UnitTests.Domain;

public sealed class TransactionTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var transaction = new Transaction(Guid.NewGuid(), Guid.NewGuid(), 120m, DateTime.UtcNow, TransactionType.Income, "Salary");

        transaction.Amount.Should().Be(120m);
        transaction.Description.Should().Be("Salary");
        transaction.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Update_ShouldMutateTransaction()
    {
        var transaction = new Transaction(Guid.NewGuid(), Guid.NewGuid(), 120m, DateTime.UtcNow.AddDays(-1), TransactionType.Income, "Salary");
        var newCategoryId = Guid.NewGuid();

        transaction.Update(newCategoryId, 80m, DateTime.UtcNow, TransactionType.Expense, "Groceries");

        transaction.CategoryId.Should().Be(newCategoryId);
        transaction.Amount.Should().Be(80m);
        transaction.Type.Should().Be(TransactionType.Expense);
        transaction.Description.Should().Be("Groceries");
        transaction.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Delete_ShouldSoftDeleteTransaction()
    {
        var transaction = new Transaction(Guid.NewGuid(), Guid.NewGuid(), 120m, DateTime.UtcNow, TransactionType.Income, "Salary");

        transaction.Delete();

        transaction.IsDeleted.Should().BeTrue();
        transaction.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void PrivateConstructor_AndUserNavigation_ShouldBeAccessibleForMaterialization()
    {
        var transaction = (Transaction)Activator.CreateInstance(typeof(Transaction), nonPublic: true)!;

        transaction.User.Should().BeNull();
    }
}
