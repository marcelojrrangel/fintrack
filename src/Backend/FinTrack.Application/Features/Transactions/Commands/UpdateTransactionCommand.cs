using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Helpers;
using FinTrack.Application.Common.Models;
using FinTrack.Application.Features.Transactions.Common;
using FinTrack.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Application.Features.Transactions.Commands;

public sealed record UpdateTransactionCommand(
    Guid Id,
    Guid CategoryId,
    decimal Amount,
    DateTime TransactionDateUtc,
    TransactionType Type,
    string Description) : IRequest<TransactionDto>, ITransactionMutationCommand;

public sealed class UpdateTransactionCommandValidator : TransactionCommandValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}

public sealed class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateTransactionCommandHandler> _logger;

    public UpdateTransactionCommandHandler(
        IFinTrackDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<UpdateTransactionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);

        _logger.LogInformation(
            "Iniciando atualização de transação. TransactionId: {TransactionId}, UserId: {UserId}, CategoryId: {CategoryId}, Amount: {Amount}",
            request.Id,
            userId,
            request.CategoryId,
            request.Amount);

        try
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Usuário não encontrado durante atualização de transação. UserId: {UserId}", userId);
                throw new NotFoundException($"User '{userId}' was not found.");
            }

            var transaction = await _dbContext.Transactions
                .Include(currentTransaction => currentTransaction.Category)
                .SingleOrDefaultAsync(
                    currentTransaction => currentTransaction.Id == request.Id &&
                                          currentTransaction.UserId == userId &&
                                          !currentTransaction.IsDeleted,
                    cancellationToken);

            if (transaction is null)
            {
                _logger.LogWarning(
                    "Transação não encontrada ou não pertence ao usuário. TransactionId: {TransactionId}, UserId: {UserId}",
                    request.Id,
                    userId);
                throw new NotFoundException($"Transaction '{request.Id}' was not found.");
            }

            var category = await _dbContext.Categories
                .SingleOrDefaultAsync(currentCategory => currentCategory.Id == request.CategoryId && currentCategory.UserId == userId, cancellationToken);

            if (category is null)
            {
                _logger.LogWarning(
                    "Categoria não encontrada ou não pertence ao usuário. UserId: {UserId}, CategoryId: {CategoryId}",
                    userId,
                    request.CategoryId);
                throw new NotFoundException($"Category '{request.CategoryId}' was not found for the current user.");
            }

            var previousValues = TransactionMapping.SerializeSnapshot(transaction);

            transaction.Update(request.CategoryId, request.Amount, request.TransactionDateUtc, request.Type, request.Description);

            var history = user.AddHistory(
                transaction,
                HistoryActionType.Updated,
                "Transaction updated.",
                previousValues,
                TransactionMapping.SerializeSnapshot(transaction));

            await _dbContext.TransactionHistories.AddAsync(history, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Transação atualizada com sucesso. TransactionId: {TransactionId}, UserId: {UserId}, NewAmount: {Amount}",
                transaction.Id,
                userId,
                request.Amount);

            // Log de auditoria para o histórico
            _logger.LogInformation(
                "AUDITORIA: Histórico de transação registrado. TransactionId: {TransactionId}, UserId: {UserId}, Action: {Action}, HistoryId: {HistoryId}",
                transaction.Id,
                userId,
                HistoryActionType.Updated,
                history.Id);

            return TransactionMapping.ToDto(transaction, category.Name);
        }
        catch (NotFoundException)
        {
            // Já logado acima, apenas propagar
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao atualizar transação. TransactionId: {TransactionId}, UserId: {UserId}",
                request.Id,
                userId);
            throw;
        }
    }
}
