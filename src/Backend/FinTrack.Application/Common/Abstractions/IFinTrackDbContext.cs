using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Common.Abstractions;

public interface IFinTrackDbContext
{
    DbSet<User> Users { get; }

    DbSet<Category> Categories { get; }

    DbSet<Transaction> Transactions { get; }

    DbSet<TransactionHistory> TransactionHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
