using FinTrack.Domain.Enums;
using FluentValidation;

namespace FinTrack.Application.Features.Transactions.Common;

public abstract class TransactionCommandValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : ITransactionMutationCommand
{
    protected TransactionCommandValidator()
    {
        RuleFor(command => command.CategoryId)
            .NotEmpty();

        RuleFor(command => command.Amount)
            .GreaterThan(0);

        RuleFor(command => command.TransactionDateUtc)
            .NotEmpty();

        RuleFor(command => command.Type)
            .IsInEnum()
            .Must(type => type != 0)
            .WithMessage("Transaction type must be either Income or Expense.");

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(250);
    }
}
