using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("CATEGORIES");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .ValueGeneratedNever();

        builder.Property(category => category.UserId)
            .HasColumnType("VARCHAR2(36)")
            .HasConversion(
                guid => guid.ToString(),
                str => Guid.Parse(str))
            .IsRequired();

        builder.Property(category => category.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(category => category.Description)
            .HasMaxLength(250);

        builder.Property(category => category.CreatedAtUtc)
            .IsRequired();

        builder.Property(category => category.UpdatedAtUtc);

        builder.HasOne(category => category.User)
            .WithMany(user => user.Categories)
            .HasForeignKey(category => category.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(Category.Transactions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(
            new
            {
                Id = DevelopmentSeedData.IncomeCategoryId,
                UserId = DevelopmentSeedData.UserId,
                Name = "Salary",
                Description = "Default income category",
                CreatedAtUtc = DevelopmentSeedData.SeededAtUtc,
                UpdatedAtUtc = (DateTime?)null
            },
            new
            {
                Id = DevelopmentSeedData.ExpenseCategoryId,
                UserId = DevelopmentSeedData.UserId,
                Name = "Bills",
                Description = "Default expense category",
                CreatedAtUtc = DevelopmentSeedData.SeededAtUtc,
                UpdatedAtUtc = (DateTime?)null
            },
            new
            {
                Id = DevelopmentSeedData.SavingsCategoryId,
                UserId = DevelopmentSeedData.UserId,
                Name = "Savings",
                Description = "Default savings category",
                CreatedAtUtc = DevelopmentSeedData.SeededAtUtc,
                UpdatedAtUtc = (DateTime?)null
            });
    }
}
