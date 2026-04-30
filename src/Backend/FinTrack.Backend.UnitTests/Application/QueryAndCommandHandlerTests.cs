using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Features.Dashboard.Queries;
using FinTrack.Application.Features.Transactions.Commands;
using FinTrack.Application.Features.Transactions.Queries;
using FinTrack.Backend.UnitTests.Testing;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinTrack.Backend.UnitTests.Application;

public sealed class QueryAndCommandHandlerTests
{
    [Fact]
    public async Task GetDashboardQuery_ShouldCalculateGreenDashboard()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, expenseCategory) = TestDbContextFactory.SeedUserWithCategories(context);
        var now = DateTime.UtcNow;
        TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 200m, now, TransactionType.Income, "Salary");
        TestDbContextFactory.SeedTransaction(context, user, expenseCategory, 50m, now, TransactionType.Expense, "Bills");
        var deleted = TestDbContextFactory.SeedTransaction(context, user, expenseCategory, 100m, now, TransactionType.Expense, "Ignored");
        deleted.Delete();
        context.SaveChanges();

        var handler = new GetDashboardQueryHandler(context, new TestCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        result.CurrentBalance.Should().Be(150m);
        result.TotalIncomeMonth.Should().Be(200m);
        result.TotalExpenseMonth.Should().Be(50m);
        result.CardColor.Should().Be("green");
    }

    [Fact]
    public async Task GetDashboardQuery_ShouldReturnRedWhenBalanceIsNegative()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, expenseCategory) = TestDbContextFactory.SeedUserWithCategories(context);
        TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 10m, DateTime.UtcNow.AddMonths(-1), TransactionType.Income, "Old salary");
        TestDbContextFactory.SeedTransaction(context, user, expenseCategory, 100m, DateTime.UtcNow, TransactionType.Expense, "Current bill");

        var handler = new GetDashboardQueryHandler(context, new TestCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        result.CurrentBalance.Should().Be(-90m);
        result.TotalIncomeMonth.Should().Be(0m);
        result.TotalExpenseMonth.Should().Be(100m);
        result.CardColor.Should().Be("red");
    }

    [Fact]
    public async Task CreateTransactionCommandHandler_ShouldPersistTransactionAndHistory()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var handler = new CreateTransactionCommandHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<CreateTransactionCommandHandler>.Instance);

        var result = await handler.Handle(
            new CreateTransactionCommand(incomeCategory.Id, 99m, DateTime.UtcNow, TransactionType.Income, "Salary"),
            CancellationToken.None);

        result.CategoryName.Should().Be(incomeCategory.Name);
        context.Transactions.Should().ContainSingle();
        context.TransactionHistories.Should().ContainSingle(history => history.Action == HistoryActionType.Created);
    }

    [Fact]
    public async Task CreateTransactionCommandHandler_ShouldThrowForMissingUser()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new CreateTransactionCommandHandler(context, new TestCurrentUserService { UserId = Guid.NewGuid() }, NullLogger<CreateTransactionCommandHandler>.Instance);

        var action = async () => await handler.Handle(
            new CreateTransactionCommand(Guid.NewGuid(), 99m, DateTime.UtcNow, TransactionType.Income, "Salary"),
            CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User '*' was not found.");
    }

    [Fact]
    public async Task CreateTransactionCommandHandler_ShouldThrowForMissingCategory()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, _, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var handler = new CreateTransactionCommandHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<CreateTransactionCommandHandler>.Instance);

        var action = async () => await handler.Handle(
            new CreateTransactionCommand(Guid.NewGuid(), 99m, DateTime.UtcNow, TransactionType.Income, "Salary"),
            CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Category '*' was not found for the current user.");
    }

    [Fact]
    public async Task UpdateTransactionCommandHandler_ShouldUpdateTransactionAndAddHistory()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, expenseCategory) = TestDbContextFactory.SeedUserWithCategories(context);
        var transaction = TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 99m, DateTime.UtcNow.AddDays(-1), TransactionType.Income, "Salary");
        var handler = new UpdateTransactionCommandHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<UpdateTransactionCommandHandler>.Instance);

        var result = await handler.Handle(
            new UpdateTransactionCommand(transaction.Id, expenseCategory.Id, 50m, DateTime.UtcNow, TransactionType.Expense, "Bills"),
            CancellationToken.None);

        result.CategoryId.Should().Be(expenseCategory.Id);
        result.CategoryName.Should().Be(expenseCategory.Name);
        context.TransactionHistories.Should().Contain(history => history.Action == HistoryActionType.Updated);
    }

    [Fact]
    public async Task UpdateTransactionCommandHandler_ShouldThrowForMissingUser()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new UpdateTransactionCommandHandler(context, new TestCurrentUserService { UserId = Guid.NewGuid() }, NullLogger<UpdateTransactionCommandHandler>.Instance);

        var action = async () => await handler.Handle(
            new UpdateTransactionCommand(Guid.NewGuid(), Guid.NewGuid(), 50m, DateTime.UtcNow, TransactionType.Expense, "Bills"),
            CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User '*' was not found.");
    }

    [Fact]
    public async Task UpdateTransactionCommandHandler_ShouldThrowForMissingTransaction()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var handler = new UpdateTransactionCommandHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<UpdateTransactionCommandHandler>.Instance);

        var action = async () => await handler.Handle(
            new UpdateTransactionCommand(Guid.NewGuid(), incomeCategory.Id, 50m, DateTime.UtcNow, TransactionType.Expense, "Bills"),
            CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Transaction '*' was not found.");
    }

    [Fact]
    public async Task UpdateTransactionCommandHandler_ShouldThrowForMissingCategory()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var transaction = TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 99m, DateTime.UtcNow, TransactionType.Income, "Salary");
        var handler = new UpdateTransactionCommandHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<UpdateTransactionCommandHandler>.Instance);

        var action = async () => await handler.Handle(
            new UpdateTransactionCommand(transaction.Id, Guid.NewGuid(), 50m, DateTime.UtcNow, TransactionType.Expense, "Bills"),
            CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Category '*' was not found for the current user.");
    }

    [Fact]
    public async Task DeleteTransactionCommandHandler_ShouldSoftDeleteAndReturnTransaction()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, _, expenseCategory) = TestDbContextFactory.SeedUserWithCategories(context);
        var transaction = TestDbContextFactory.SeedTransaction(context, user, expenseCategory, 99m, DateTime.UtcNow, TransactionType.Expense, "Bills");
        var handler = new DeleteTransactionCommandHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<DeleteTransactionCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteTransactionCommand(transaction.Id), CancellationToken.None);

        result.IsDeleted.Should().BeTrue();
        result.CategoryName.Should().Be(expenseCategory.Name);
        context.TransactionHistories.Should().Contain(history => history.Action == HistoryActionType.Deleted);
    }

    [Fact]
    public async Task DeleteTransactionCommandHandler_ShouldThrowForMissingUser()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new DeleteTransactionCommandHandler(context, new TestCurrentUserService { UserId = Guid.NewGuid() }, NullLogger<DeleteTransactionCommandHandler>.Instance);

        var action = async () => await handler.Handle(new DeleteTransactionCommand(Guid.NewGuid()), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User '*' was not found.");
    }

    [Fact]
    public async Task DeleteTransactionCommandHandler_ShouldThrowForMissingTransaction()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, _, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var handler = new DeleteTransactionCommandHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<DeleteTransactionCommandHandler>.Instance);

        var action = async () => await handler.Handle(new DeleteTransactionCommand(Guid.NewGuid()), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Transaction '*' was not found.");
    }

    [Fact]
    public async Task GetTransactionsQueryHandler_ShouldReturnOrderedTransactions()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var newer = TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 20m, DateTime.UtcNow, TransactionType.Income, "New");
        var older = TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 10m, DateTime.UtcNow.AddDays(-1), TransactionType.Income, "Old");
        var handler = new GetTransactionsQueryHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<GetTransactionsQueryHandler>.Instance);

        var result = await handler.Handle(new GetTransactionsQuery(1, 5), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.First().Description.Should().Be("New");
        result.Items.First().CategoryName.Should().Be(incomeCategory.Name);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetTransactionsQueryHandler_ShouldThrowWhenUserDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new GetTransactionsQueryHandler(context, new TestCurrentUserService { UserId = Guid.NewGuid() }, NullLogger<GetTransactionsQueryHandler>.Instance);

        var action = async () => await handler.Handle(new GetTransactionsQuery(1, 5), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User '*' was not found.");
    }

    [Fact]
    public async Task GetTransactionsQueryHandler_ShouldPaginateCorrectly()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, _) = TestDbContextFactory.SeedUserWithCategories(context);

        // Criar 12 transações para testar paginação
        for (int i = 0; i < 12; i++)
        {
            TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 100m + i, DateTime.UtcNow.AddDays(-i), TransactionType.Income, $"Transaction {i}");
        }

        var handler = new GetTransactionsQueryHandler(context, new TestCurrentUserService { UserId = user.Id }, NullLogger<GetTransactionsQueryHandler>.Instance);

        // Testar primeira página (5 itens)
        var page1 = await handler.Handle(new GetTransactionsQuery(1, 5), CancellationToken.None);
        page1.Items.Should().HaveCount(5);
        page1.TotalCount.Should().Be(12);
        page1.TotalPages.Should().Be(3);
        page1.PageNumber.Should().Be(1);
        page1.HasNextPage.Should().BeTrue();
        page1.HasPreviousPage.Should().BeFalse();

        // Testar segunda página (5 itens)
        var page2 = await handler.Handle(new GetTransactionsQuery(2, 5), CancellationToken.None);
        page2.Items.Should().HaveCount(5);
        page2.PageNumber.Should().Be(2);
        page2.HasNextPage.Should().BeTrue();
        page2.HasPreviousPage.Should().BeTrue();

        // Testar terceira página (2 itens restantes)
        var page3 = await handler.Handle(new GetTransactionsQuery(3, 5), CancellationToken.None);
        page3.Items.Should().HaveCount(2);
        page3.PageNumber.Should().Be(3);
        page3.HasNextPage.Should().BeFalse();
        page3.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetTransactionByIdQueryHandler_ShouldReturnTransactionOrThrow()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var transaction = TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 20m, DateTime.UtcNow, TransactionType.Income, "New");
        var handler = new GetTransactionByIdQueryHandler(context, new TestCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new GetTransactionByIdQuery(transaction.Id), CancellationToken.None);

        result.Description.Should().Be("New");
        result.CategoryName.Should().Be(incomeCategory.Name);

        var action = async () => await handler.Handle(new GetTransactionByIdQuery(Guid.NewGuid()), CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Transaction '*' was not found.");
    }

    [Fact]
    public async Task GetTransactionHistoryQueryHandler_ShouldReturnOrderedHistoryOrThrow()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var (user, incomeCategory, _) = TestDbContextFactory.SeedUserWithCategories(context);
        var transaction = TestDbContextFactory.SeedTransaction(context, user, incomeCategory, 99m, DateTime.UtcNow, TransactionType.Income, "Salary");
        var first = user.AddHistory(transaction, HistoryActionType.Created, "Created");
        await Task.Delay(20);
        var second = user.AddHistory(transaction, HistoryActionType.Updated, "Updated");
        context.TransactionHistories.AddRange(first, second);
        context.SaveChanges();
        var handler = new GetTransactionHistoryQueryHandler(context, new TestCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new GetTransactionHistoryQuery(transaction.Id), CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Action.Should().Be(HistoryActionType.Updated);

        var action = async () => await handler.Handle(new GetTransactionHistoryQuery(Guid.NewGuid()), CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Transaction '*' was not found.");
    }
}
