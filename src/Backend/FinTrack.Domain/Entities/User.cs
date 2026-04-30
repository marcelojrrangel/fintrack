using FinTrack.Domain.Common;
using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public sealed class User : AuditableEntity
{
    private readonly List<Category> _categories = [];
    private readonly List<Transaction> _transactions = [];
    private readonly List<TransactionHistory> _transactionHistory = [];

    private User()
    {
    }

    public User(string fullName, string email)
    {
        FullName = Guard.AgainstNullOrWhiteSpace(fullName, nameof(fullName), 120);
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email), 180);
    }

    public string FullName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public IReadOnlyCollection<TransactionHistory> TransactionHistory => _transactionHistory.AsReadOnly();

    public void UpdateProfile(string fullName, string email)
    {
        FullName = Guard.AgainstNullOrWhiteSpace(fullName, nameof(fullName), 120);
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email), 180);
        MarkAsUpdated();
    }

    public Category AddCategory(string name, string? description = null)
    {
        var category = new Category(Id, name, description);
        _categories.Add(category);
        MarkAsUpdated();
        return category;
    }

    public Transaction AddTransaction(
        Category category,
        decimal amount,
        DateTime transactionDateUtc,
        TransactionType type,
        string description)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (category.UserId != Id)
        {
            throw new InvalidOperationException("A category must belong to the same user as the transaction.");
        }

        var transaction = new Transaction(Id, category.Id, amount, transactionDateUtc, type, description);
        _transactions.Add(transaction);
        category.RegisterTransaction(transaction);
        MarkAsUpdated();
        return transaction;
    }

    public TransactionHistory AddHistory(
        Transaction transaction,
        HistoryActionType action,
        string description,
        string? previousValues = null,
        string? currentValues = null)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction.UserId != Id)
        {
            throw new InvalidOperationException("A transaction history entry must belong to the same user.");
        }

        var historyItem = new TransactionHistory(Id, transaction.Id, action, description, previousValues, currentValues);
        _transactionHistory.Add(historyItem);
        transaction.RegisterHistory(historyItem);
        MarkAsUpdated();
        return historyItem;
    }
}
