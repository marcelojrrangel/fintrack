using FinTrack.Application.Common.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence;

public sealed class OracleDatabaseInitializer : IDatabaseInitializer
{
    private readonly FinTrackDbContext _dbContext;

    public OracleDatabaseInitializer(FinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Aplicar migrations pendentes
        await _dbContext.Database.MigrateAsync(cancellationToken);
    }
}
