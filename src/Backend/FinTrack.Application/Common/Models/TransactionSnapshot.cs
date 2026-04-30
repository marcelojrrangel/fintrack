using FinTrack.Domain.Enums;

namespace FinTrack.Application.Common.Models;

public sealed record TransactionSnapshot(
    Guid Id,
    Guid UserId,
    Guid CategoryId,
    decimal Amount,
    DateTime TransactionDateUtc,
    TransactionType Type,
    string Description,
    bool IsDeleted);
