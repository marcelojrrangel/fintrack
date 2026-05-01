using System.Text.Json.Serialization;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Common.Models;

public sealed record TransactionDto(
    Guid Id,
    Guid UserId,
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    [property: JsonPropertyName("transactionDateUtc")] DateTime TransactionDateUtc,
    [property: JsonPropertyName("type")] TransactionType Type,
    [property: JsonPropertyName("description")] string Description,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
