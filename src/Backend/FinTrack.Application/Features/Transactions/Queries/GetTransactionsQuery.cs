using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Application.Features.Transactions.Queries;

public sealed record GetTransactionsQuery(int PageNumber = 1, int PageSize = 5) : IRequest<PagedResponse<TransactionDto>>;

public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PagedResponse<TransactionDto>>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTransactionsQueryHandler> _logger;

    public GetTransactionsQueryHandler(
        IFinTrackDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<GetTransactionsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResponse<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);
        await UserGuard.EnsureExistsAsync(_dbContext, userId, cancellationToken);

        _logger.LogInformation(
            "Buscando transações paginadas para o usuário. UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            userId, request.PageNumber, request.PageSize);

        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId && !transaction.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            _logger.LogInformation("Nenhuma transação encontrada para o usuário. UserId: {UserId}", userId);
            return PagedResponse<TransactionDto>.Empty(request.PageNumber, request.PageSize);
        }

        var items = await query
            .OrderByDescending(transaction => transaction.TransactionDateUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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

        _logger.LogInformation(
            "Transações recuperadas com sucesso. UserId: {UserId}, TotalCount: {TotalCount}, ItemsReturned: {ItemsReturned}",
            userId, totalCount, items.Count);

        return PagedResponse<TransactionDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}
