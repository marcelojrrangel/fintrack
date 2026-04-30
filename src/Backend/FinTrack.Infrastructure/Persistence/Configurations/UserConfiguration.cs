using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("USERS");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .ValueGeneratedNever();

        builder.Property(user => user.FullName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();

        builder.Property(user => user.UpdatedAtUtc);

        builder.Metadata.FindNavigation(nameof(User.Categories))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(User.Transactions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(User.TransactionHistory))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(new
        {
            Id = DevelopmentSeedData.UserId,
            FullName = "FinTrack Demo User",
            Email = "demo@fintrack.local",
            CreatedAtUtc = DevelopmentSeedData.SeededAtUtc,
            UpdatedAtUtc = (DateTime?)null
        });
    }
}
