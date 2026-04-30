using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FluentAssertions;

namespace FinTrack.Backend.UnitTests.Domain;

public sealed class UserTests
{
    [Fact]
    public void Constructor_AndUpdateProfile_ShouldNormalizeValues()
    {
        var user = new User("  Jane Doe  ", "  jane@fintrack.dev  ");

        user.FullName.Should().Be("Jane Doe");
        user.Email.Should().Be("jane@fintrack.dev");
        user.UpdatedAtUtc.Should().BeNull();

        user.UpdateProfile("  Mary Doe  ", "  mary@fintrack.dev  ");

        user.FullName.Should().Be("Mary Doe");
        user.Email.Should().Be("mary@fintrack.dev");
        user.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void AddCategory_ShouldCreateAndRegisterCategory()
    {
        var user = new User("Jane", "jane@fintrack.dev");

        var category = user.AddCategory("  Salary  ", "  Monthly income  ");

        category.UserId.Should().Be(user.Id);
        category.Name.Should().Be("Salary");
        category.Description.Should().Be("Monthly income");
        user.Categories.Should().ContainSingle().Which.Should().Be(category);
        user.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void AddTransaction_ShouldCreateAndRegisterTransaction()
    {
        var user = new User("Jane", "jane@fintrack.dev");
        var category = user.AddCategory("Salary");

        var transaction = user.AddTransaction(category, 100m, DateTime.UtcNow, TransactionType.Income, "Salary payment");

        transaction.UserId.Should().Be(user.Id);
        transaction.CategoryId.Should().Be(category.Id);
        user.Transactions.Should().ContainSingle().Which.Should().Be(transaction);
        category.Transactions.Should().ContainSingle().Which.Should().Be(transaction);
    }

    [Fact]
    public void AddTransaction_ShouldThrowWhenCategoryIsNull()
    {
        var user = new User("Jane", "jane@fintrack.dev");

        var action = () => user.AddTransaction(null!, 100m, DateTime.UtcNow, TransactionType.Income, "Salary payment");

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddTransaction_ShouldThrowWhenCategoryBelongsToAnotherUser()
    {
        var user = new User("Jane", "jane@fintrack.dev");
        var anotherUser = new User("John", "john@fintrack.dev");
        var category = anotherUser.AddCategory("Salary");

        var action = () => user.AddTransaction(category, 100m, DateTime.UtcNow, TransactionType.Income, "Salary payment");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("A category must belong to the same user as the transaction.");
    }

    [Fact]
    public void AddHistory_ShouldCreateAndRegisterHistory()
    {
        var user = new User("Jane", "jane@fintrack.dev");
        var category = user.AddCategory("Salary");
        var transaction = user.AddTransaction(category, 100m, DateTime.UtcNow, TransactionType.Income, "Salary payment");

        var history = user.AddHistory(transaction, HistoryActionType.Created, "Created", "before", "after");

        history.UserId.Should().Be(user.Id);
        history.TransactionId.Should().Be(transaction.Id);
        user.TransactionHistory.Should().ContainSingle().Which.Should().Be(history);
        transaction.HistoryEntries.Should().ContainSingle().Which.Should().Be(history);
    }

    [Fact]
    public void AddHistory_ShouldThrowWhenTransactionIsNull()
    {
        var user = new User("Jane", "jane@fintrack.dev");

        var action = () => user.AddHistory(null!, HistoryActionType.Created, "Created");

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddHistory_ShouldThrowWhenTransactionBelongsToAnotherUser()
    {
        var user = new User("Jane", "jane@fintrack.dev");
        var anotherUser = new User("John", "john@fintrack.dev");
        var category = anotherUser.AddCategory("Salary");
        var transaction = anotherUser.AddTransaction(category, 100m, DateTime.UtcNow, TransactionType.Income, "Salary payment");

        var action = () => user.AddHistory(transaction, HistoryActionType.Created, "Created");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("A transaction history entry must belong to the same user.");
    }
}
