using FinTrack.Domain.Enums;

namespace FinTrack.Application.Common.Models;

public sealed record TransactionHistoryDto(
    Guid Id,
    Guid TransactionId,
    HistoryActionType Action,
    string Description,
    string? PreviousValues,
    string? CurrentValues,
    DateTime OccurredAtUtc);
