using FinTrack.Domain.Enums;

namespace FinTrack.Application.Common.Models;

public sealed record TransactionMutationRequest(
    Guid CategoryId,
    decimal Amount,
    DateTime TransactionDateUtc,
    TransactionType Type,
    string Description);
