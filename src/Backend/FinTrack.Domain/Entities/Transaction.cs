using FinTrack.Domain.Common;
using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public sealed class Transaction : AuditableEntity
{
    private readonly List<TransactionHistory> _historyEntries = [];

    private Transaction()
    {
    }

    public Transaction(
        Guid userId,
        Guid categoryId,
        decimal amount,
        DateTime transactionDateUtc,
        TransactionType type,
        string description)
    {
        UserId = userId;
        CategoryId = categoryId;
        Amount = Guard.AgainstNegativeOrZero(amount, nameof(amount));
        TransactionDateUtc = transactionDateUtc;
        Type = type;
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 250);
    }

    public Guid UserId { get; private set; }

    public User? User { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime TransactionDateUtc { get; private set; }

    public TransactionType Type { get; private set; }

    public string Description { get; private set; } = null!;

    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<TransactionHistory> HistoryEntries => _historyEntries.AsReadOnly();

    public void Update(Guid categoryId, decimal amount, DateTime transactionDateUtc, TransactionType type, string description)
    {
        CategoryId = categoryId;
        Amount = Guard.AgainstNegativeOrZero(amount, nameof(amount));
        TransactionDateUtc = transactionDateUtc;
        Type = type;
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 250);
        MarkAsUpdated();
    }

    public void Delete()
    {
        IsDeleted = true;
        MarkAsUpdated();
    }

    internal void RegisterHistory(TransactionHistory historyEntry)
    {
        ArgumentNullException.ThrowIfNull(historyEntry);
        _historyEntries.Add(historyEntry);
        MarkAsUpdated();
    }
}
