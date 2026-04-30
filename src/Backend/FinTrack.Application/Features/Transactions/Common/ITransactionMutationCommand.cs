using FinTrack.Domain.Enums;

namespace FinTrack.Application.Features.Transactions.Common;

public interface ITransactionMutationCommand
{
    Guid CategoryId { get; }

    decimal Amount { get; }

    DateTime TransactionDateUtc { get; }

    TransactionType Type { get; }

    string Description { get; }
}
