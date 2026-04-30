using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Backend.UnitTests.Testing;

internal static class TestDbContextFactory
{
    public static FinTrackDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new FinTrackDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static (User User, Category IncomeCategory, Category ExpenseCategory) SeedUserWithCategories(FinTrackDbContext context)
    {
        var user = new User("Demo User", "demo@local.test");
        var incomeCategory = user.AddCategory("Salary", "Income");
        var expenseCategory = user.AddCategory("Bills", "Expense");

        context.Users.Add(user);
        context.SaveChanges();

        return (user, incomeCategory, expenseCategory);
    }

    public static Transaction SeedTransaction(
        FinTrackDbContext context,
        User user,
        Category category,
        decimal amount,
        DateTime transactionDateUtc,
        TransactionType type,
        string description)
    {
        var transaction = user.AddTransaction(category, amount, transactionDateUtc, type, description);
        context.Transactions.Add(transaction);
        context.SaveChanges();
        return transaction;
    }
}
