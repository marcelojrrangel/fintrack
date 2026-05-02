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

public sealed record CreateTransactionCommand(
    Guid CategoryId,
    decimal Amount,
    DateTime TransactionDateUtc,
    TransactionType Type,
    string Description) : IRequest<TransactionDto>, ITransactionMutationCommand;

public sealed class CreateTransactionCommandValidator : TransactionCommandValidator<CreateTransactionCommand>;

public sealed class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly IFinTrackDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(
        IFinTrackDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserAccessor.GetRequiredUserId(_currentUserService);

        _logger.LogInformation(
            "Starting transaction creation. UserId: {UserId}, CategoryId: {CategoryId}, Amount: {Amount}, Type: {Type}",
            userId,
            request.CategoryId,
            request.Amount,
            request.Type);

        try
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Usuário não encontrado durante criação de transação. UserId: {UserId}", userId);
                throw new NotFoundException($"User '{userId}' was not found.");
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

            var transaction = user.AddTransaction(category, request.Amount, request.TransactionDateUtc, request.Type, request.Description);
            var history = user.AddHistory(
                transaction,
                HistoryActionType.Created,
                "Transaction created.",
                null,
                TransactionMapping.SerializeSnapshot(transaction));

            await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
            await _dbContext.TransactionHistories.AddAsync(history, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Transação criada com sucesso. TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}, Type: {Type}",
                transaction.Id,
                userId,
                request.Amount,
                request.Type);

            // Log de auditoria para o histórico
            _logger.LogInformation(
                "AUDITORIA: Histórico de transação registrado. TransactionId: {TransactionId}, UserId: {UserId}, Action: {Action}, HistoryId: {HistoryId}",
                transaction.Id,
                userId,
                HistoryActionType.Created,
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
                "Erro ao criar transação. UserId: {UserId}, CategoryId: {CategoryId}, Amount: {Amount}",
                userId,
                request.CategoryId,
                request.Amount);
            throw;
        }
    }
}
