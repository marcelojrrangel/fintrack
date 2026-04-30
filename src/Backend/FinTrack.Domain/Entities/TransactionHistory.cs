using FinTrack.Domain.Common;
using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public sealed class TransactionHistory : Entity
{
    private TransactionHistory()
    {
    }

    public TransactionHistory(
        Guid userId,
        Guid transactionId,
        HistoryActionType action,
        string description,
        string? previousValues = null,
        string? currentValues = null)
    {
        UserId = userId;
        TransactionId = transactionId;
        Action = action;
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 250);
        PreviousValues = NormalizePayload(previousValues);
        CurrentValues = NormalizePayload(currentValues);
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public User? User { get; private set; }

    public Guid TransactionId { get; private set; }

    public Transaction? Transaction { get; private set; }

    public HistoryActionType Action { get; private set; }

    public string Description { get; private set; } = null!;

    public string? PreviousValues { get; private set; }

    public string? CurrentValues { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    private static string? NormalizePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        return Guard.AgainstNullOrWhiteSpace(payload, nameof(payload), 4000);
    }
}
