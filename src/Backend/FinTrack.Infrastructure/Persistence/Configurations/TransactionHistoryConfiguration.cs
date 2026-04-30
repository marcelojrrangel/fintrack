using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

internal sealed class TransactionHistoryConfiguration : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.ToTable("TRANSACTION_HISTORY");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .ValueGeneratedNever();

        builder.Property(history => history.UserId)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .IsRequired();

        builder.Property(history => history.TransactionId)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .IsRequired();

        builder.Property(history => history.Action)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(history => history.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(history => history.PreviousValues)
            .HasColumnType("CLOB");

        builder.Property(history => history.CurrentValues)
            .HasColumnType("CLOB");

        builder.Property(history => history.OccurredAtUtc)
            .IsRequired();

        builder.HasOne(history => history.User)
            .WithMany(user => user.TransactionHistory)
            .HasForeignKey(history => history.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(history => history.Transaction)
            .WithMany(transaction => transaction.HistoryEntries)
            .HasForeignKey(history => history.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
