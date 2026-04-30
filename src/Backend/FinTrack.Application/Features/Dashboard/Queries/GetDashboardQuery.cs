using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Features.Dashboard.Queries;

public sealed record GetDashboardQuery : IRequest<DashboardDto>;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardQueryHandler(IFinTrackDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);
        await UserGuard.EnsureExistsAsync(_dbContext, userId, cancellationToken);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        var activeTransactions = _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId && !transaction.IsDeleted);

        var totalIncome = await activeTransactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Select(transaction => (decimal?)transaction.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var totalExpense = await activeTransactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Select(transaction => (decimal?)transaction.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var totalIncomeMonth = await activeTransactions
            .Where(transaction =>
                transaction.Type == TransactionType.Income &&
                transaction.TransactionDateUtc >= monthStart &&
                transaction.TransactionDateUtc < nextMonthStart)
            .Select(transaction => (decimal?)transaction.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var totalExpenseMonth = await activeTransactions
            .Where(transaction =>
                transaction.Type == TransactionType.Expense &&
                transaction.TransactionDateUtc >= monthStart &&
                transaction.TransactionDateUtc < nextMonthStart)
            .Select(transaction => (decimal?)transaction.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var currentBalance = totalIncome - totalExpense;

        return new DashboardDto(
            currentBalance,
            totalIncomeMonth,
            totalExpenseMonth,
            currentBalance < 0 ? "red" : "green");
    }
}
