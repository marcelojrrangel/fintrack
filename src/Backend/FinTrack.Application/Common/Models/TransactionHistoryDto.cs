using System.Text.Json.Serialization;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Common.Models;

public sealed record TransactionHistoryDto(
    Guid Id,
    Guid TransactionId,
    [property: JsonPropertyName("action")] HistoryActionType Action,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("previousValues")] string? PreviousValues,
    [property: JsonPropertyName("currentValues")] string? CurrentValues,
    [property: JsonPropertyName("occurredAtUtc")] DateTime OccurredAtUtc);
