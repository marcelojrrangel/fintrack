using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Features.Transactions.Queries;

public sealed record GetTransactionHistoryQuery(Guid TransactionId) : IRequest<IReadOnlyCollection<TransactionHistoryDto>>;

public sealed class GetTransactionHistoryQueryValidator : AbstractValidator<GetTransactionHistoryQuery>
{
    public GetTransactionHistoryQueryValidator()
    {
        RuleFor(query => query.TransactionId)
            .NotEmpty();
    }
}

public sealed class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, IReadOnlyCollection<TransactionHistoryDto>>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionHistoryQueryHandler(IFinTrackDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyCollection<TransactionHistoryDto>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);

        var transactionExists = await _dbContext.Transactions
            .AsNoTracking()
            .AnyAsync(transaction => transaction.Id == request.TransactionId && transaction.UserId == userId, cancellationToken);

        if (!transactionExists)
        {
            throw new NotFoundException($"Transaction '{request.TransactionId}' was not found.");
        }

        return await _dbContext.TransactionHistories
            .AsNoTracking()
            .Where(history => history.TransactionId == request.TransactionId && history.UserId == userId)
            .OrderByDescending(history => history.OccurredAtUtc)
            .Select(history => new TransactionHistoryDto(
                history.Id,
                history.TransactionId,
                history.Action,
                history.Description,
                history.PreviousValues,
                history.CurrentValues,
                history.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }
}
