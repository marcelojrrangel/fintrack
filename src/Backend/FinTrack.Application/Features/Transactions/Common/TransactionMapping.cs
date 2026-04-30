using System.Text.Json;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Features.Transactions.Common;

internal static class TransactionMapping
{
    public static TransactionDto ToDto(Transaction transaction, string categoryName) =>
        new(
            transaction.Id,
            transaction.UserId,
            transaction.CategoryId,
            categoryName,
            transaction.Amount,
            transaction.TransactionDateUtc,
            transaction.Type,
            transaction.Description,
            transaction.IsDeleted,
            transaction.CreatedAtUtc,
            transaction.UpdatedAtUtc);

    public static TransactionSnapshot ToSnapshot(Transaction transaction) =>
        new(
            transaction.Id,
            transaction.UserId,
            transaction.CategoryId,
            transaction.Amount,
            transaction.TransactionDateUtc,
            transaction.Type,
            transaction.Description,
            transaction.IsDeleted);

    public static string SerializeSnapshot(Transaction transaction) =>
        JsonSerializer.Serialize(ToSnapshot(transaction));
}
