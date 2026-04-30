using FinTrack.Application.Common.Abstractions;
using FinTrack.Backend.UnitTests.Testing;
using FinTrack.Domain.Entities;
using FinTrack.Infrastructure;
using FinTrack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Backend.UnitTests.Infrastructure;

public sealed class InfrastructureTests
{
    [Fact]
    public void AddInfrastructure_ShouldRequireConnectionString()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var action = () => services.AddInfrastructure(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Connection string 'FinTrackOracle' was not found.");
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterExpectedServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:FinTrackOracle"] = "User Id=test;Password=test;Data Source=localhost:1521/FREEPDB1;"
            })
            .Build();

        services.AddInfrastructure(configuration);

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IFinTrackDbContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IUnitOfWork));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IDatabaseInitializer));
    }

    [Fact]
    public async Task FinTrackDbContext_ShouldApplyConfigurationsAndSeedData()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Users.Count().Should().Be(1);
        context.Categories.Count().Should().Be(3);

        context.Model.FindEntityType(typeof(User))!.GetTableName().Should().Be("USERS");
        context.Model.FindEntityType(typeof(Category))!.GetTableName().Should().Be("CATEGORIES");
        context.Model.FindEntityType(typeof(Transaction))!.GetTableName().Should().Be("TRANSACTIONS");
        context.Model.FindEntityType(typeof(TransactionHistory))!.GetTableName().Should().Be("TRANSACTION_HISTORY");

        context.Model.FindEntityType(typeof(User))!.FindNavigation(nameof(User.Categories))!.GetPropertyAccessMode().Should().Be(PropertyAccessMode.Field);
        context.Model.FindEntityType(typeof(Transaction))!.FindNavigation(nameof(Transaction.HistoryEntries))!.GetPropertyAccessMode().Should().Be(PropertyAccessMode.Field);
        context.Model.FindEntityType(typeof(Transaction))!.FindProperty(nameof(Transaction.Amount))!.GetPrecision().Should().Be(18);
        context.Model.FindEntityType(typeof(TransactionHistory))!
            .FindProperty(nameof(TransactionHistory.PreviousValues))!
            .FindAnnotation(RelationalAnnotationNames.ColumnType)!.Value.Should().Be("CLOB");
    }

    [Fact]
    public async Task OracleDatabaseInitializer_ShouldEnsureDatabaseCreated()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var initializer = new OracleDatabaseInitializer(context);

        await initializer.InitializeAsync(CancellationToken.None);

        (await context.Users.AnyAsync()).Should().BeTrue();
    }
}
