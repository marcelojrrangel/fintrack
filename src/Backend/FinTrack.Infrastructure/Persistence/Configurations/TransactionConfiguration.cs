using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("TRANSACTIONS");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .ValueGeneratedNever();

        builder.Property(transaction => transaction.UserId)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .IsRequired();

        builder.Property(transaction => transaction.CategoryId)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(transaction => transaction.TransactionDateUtc)
            .IsRequired();

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // NOTA: Removida a constraint CHECK pois causa problemas com EXECUTE IMMEDIATE no Oracle
        // A validação do enum é feita na camada de aplicação

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(transaction => transaction.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(transaction => transaction.CreatedAtUtc)
            .IsRequired();

        builder.Property(transaction => transaction.UpdatedAtUtc);

        builder.HasOne(transaction => transaction.User)
            .WithMany(user => user.Transactions)
            .HasForeignKey(transaction => transaction.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transaction => transaction.Category)
            .WithMany(category => category.Transactions)
            .HasForeignKey(transaction => transaction.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(Transaction.HistoryEntries))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
