using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Features.Transactions.Queries;

public sealed record GetTransactionsQuery(
    int PageNumber = 1, 
    int PageSize = 5,
    string? Description = null,
    Guid? CategoryId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Type = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null) : IRequest<PagedResponse<TransactionDto>>;

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
            "Buscando transações filtradas para o usuário. UserId: {UserId}, Page: {PageNumber}",
            userId, request.PageNumber);

        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && !t.IsDeleted);

        // Aplicação de filtros dinâmicos
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            query = query.Where(t => t.Description.ToLower().Contains(request.Description.ToLower()));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(t => t.TransactionDateUtc >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(t => t.TransactionDateUtc <= request.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<TransactionType>(request.Type, out var typeEnum))
        {
            query = query.Where(t => t.Type == typeEnum);
        }

        if (request.MinAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= request.MaxAmount.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return PagedResponse<TransactionDto>.Empty(request.PageNumber, request.PageSize);
        }

        var items = await query
            .OrderByDescending(t => t.TransactionDateUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.UserId,
                t.CategoryId,
                t.Category != null ? t.Category.Name : string.Empty,
                t.Amount,
                t.TransactionDateUtc,
                t.Type,
                t.Description,
                t.IsDeleted,
                t.CreatedAtUtc,
                t.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResponse<TransactionDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}
