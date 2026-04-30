using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Features.Transactions.Queries;

public sealed record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionDto>;

public sealed class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty();
    }
}

public sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionByIdQueryHandler(IFinTrackDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);

        var transaction = await _dbContext.Transactions
            .AsNoTracking()
            .Where(currentTransaction =>
                currentTransaction.Id == request.Id &&
                currentTransaction.UserId == userId &&
                !currentTransaction.IsDeleted)
            .Select(currentTransaction => new TransactionDto(
                currentTransaction.Id,
                currentTransaction.UserId,
                currentTransaction.CategoryId,
                currentTransaction.Category != null ? currentTransaction.Category.Name : string.Empty,
                currentTransaction.Amount,
                currentTransaction.TransactionDateUtc,
                currentTransaction.Type,
                currentTransaction.Description,
                currentTransaction.IsDeleted,
                currentTransaction.CreatedAtUtc,
                currentTransaction.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return transaction ?? throw new NotFoundException($"Transaction '{request.Id}' was not found.");
    }
}
