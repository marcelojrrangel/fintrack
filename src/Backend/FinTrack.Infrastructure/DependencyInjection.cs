using FinTrack.Application.Common.Abstractions;
using FinTrack.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FinTrackOracle")
                               ?? throw new InvalidOperationException("Connection string 'FinTrackOracle' was not found.");

        services.AddDbContext<FinTrackDbContext>(options =>
            options.UseOracle(connectionString));

        services.AddScoped<IFinTrackDbContext>(provider => provider.GetRequiredService<FinTrackDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<FinTrackDbContext>());
        services.AddScoped<IDatabaseInitializer, OracleDatabaseInitializer>();

        return services;
    }
}
