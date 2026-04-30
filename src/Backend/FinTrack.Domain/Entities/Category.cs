using FinTrack.Domain.Common;

namespace FinTrack.Domain.Entities;

public sealed class Category : AuditableEntity
{
    private readonly List<Transaction> _transactions = [];

    private Category()
    {
    }

    public Category(Guid userId, string name, string? description = null)
    {
        UserId = userId;
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 80);
        Description = NormalizeDescription(description);
    }

    public Guid UserId { get; private set; }

    public User? User { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public void Rename(string name, string? description = null)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 80);
        Description = NormalizeDescription(description);
        MarkAsUpdated();
    }

    internal void RegisterTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        _transactions.Add(transaction);
        MarkAsUpdated();
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return Guard.AgainstNullOrWhiteSpace(description, nameof(description), 250);
    }
}
