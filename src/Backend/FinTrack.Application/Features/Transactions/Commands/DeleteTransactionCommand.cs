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

public sealed record DeleteTransactionCommand(Guid Id) : IRequest<TransactionDto>;

public sealed class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}

public sealed class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, TransactionDto>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTransactionCommandHandler> _logger;

    public DeleteTransactionCommandHandler(
        IFinTrackDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<DeleteTransactionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);

        _logger.LogInformation(
            "Iniciando exclusão de transação. TransactionId: {TransactionId}, UserId: {UserId}",
            request.Id,
            userId);

        try
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Usuário não encontrado durante exclusão de transação. UserId: {UserId}", userId);
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
                    "Transação não encontrada ou já excluída. TransactionId: {TransactionId}, UserId: {UserId}",
                    request.Id,
                    userId);
                throw new NotFoundException($"Transaction '{request.Id}' was not found.");
            }

            var previousValues = TransactionMapping.SerializeSnapshot(transaction);

            transaction.Delete();

            var history = user.AddHistory(
                transaction,
                HistoryActionType.Deleted,
                "Transaction deleted.",
                previousValues,
                TransactionMapping.SerializeSnapshot(transaction));

            await _dbContext.TransactionHistories.AddAsync(history, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Transação excluída com sucesso. TransactionId: {TransactionId}, UserId: {UserId}",
                transaction.Id,
                userId);

            // Log de auditoria para o histórico
            _logger.LogInformation(
                "AUDITORIA: Histórico de transação registrado. TransactionId: {TransactionId}, UserId: {UserId}, Action: {Action}, HistoryId: {HistoryId}",
                transaction.Id,
                userId,
                HistoryActionType.Deleted,
                history.Id);

            return TransactionMapping.ToDto(transaction, transaction.Category?.Name ?? string.Empty);
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
                "Erro ao excluir transação. TransactionId: {TransactionId}, UserId: {UserId}",
                request.Id,
                userId);
            throw;
        }
    }
}
