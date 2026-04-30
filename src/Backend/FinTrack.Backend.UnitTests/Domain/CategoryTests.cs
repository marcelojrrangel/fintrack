using FinTrack.Domain.Entities;
using FluentAssertions;

namespace FinTrack.Backend.UnitTests.Domain;

public sealed class CategoryTests
{
    [Fact]
    public void Constructor_ShouldNormalizeOptionalDescription()
    {
        var category = new Category(Guid.NewGuid(), "  Bills  ", "   ");

        category.Name.Should().Be("Bills");
        category.Description.Should().BeNull();
    }

    [Fact]
    public void Rename_ShouldUpdateValuesAndTimestamp()
    {
        var category = new Category(Guid.NewGuid(), "Bills", "Expense");

        category.Rename("  Savings  ", "  Emergency fund  ");

        category.Name.Should().Be("Savings");
        category.Description.Should().Be("Emergency fund");
        category.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void PrivateConstructor_AndUserNavigation_ShouldBeAccessibleForMaterialization()
    {
        var category = (Category)Activator.CreateInstance(typeof(Category), nonPublic: true)!;

        category.User.Should().BeNull();
    }
}
