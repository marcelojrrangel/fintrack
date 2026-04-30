using FinTrack.Domain.Enums;

namespace FinTrack.Application.Common.Models;

public sealed record TransactionDto(
    Guid Id,
    Guid UserId,
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    DateTime TransactionDateUtc,
    TransactionType Type,
    string Description,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
