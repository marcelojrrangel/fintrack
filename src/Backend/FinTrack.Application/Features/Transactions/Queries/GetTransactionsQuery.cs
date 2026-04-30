using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Features.Transactions.Queries;

public sealed record GetTransactionsQuery : IRequest<IReadOnlyCollection<TransactionDto>>;

public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, IReadOnlyCollection<TransactionDto>>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionsQueryHandler(IFinTrackDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyCollection<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);
        await UserGuard.EnsureExistsAsync(_dbContext, userId, cancellationToken);

        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId && !transaction.IsDeleted)
            .OrderByDescending(transaction => transaction.TransactionDateUtc)
            .Select(transaction => new TransactionDto(
                transaction.Id,
                transaction.UserId,
                transaction.CategoryId,
                transaction.Category != null ? transaction.Category.Name : string.Empty,
                transaction.Amount,
                transaction.TransactionDateUtc,
                transaction.Type,
                transaction.Description,
                transaction.IsDeleted,
                transaction.CreatedAtUtc,
                transaction.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
