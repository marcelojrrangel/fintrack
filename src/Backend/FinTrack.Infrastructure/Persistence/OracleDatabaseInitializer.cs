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
        try
        {
            // Aplicar migrations pendentes
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.InnerException?.Message?.Contains("ORA-65040") == true)
        {
            // ORA-65040: operação não permitida em um banco de dados plugável
            // Isso é esperado em alguns casos durante migrações Oracle - ignorar
        }
    }
}
