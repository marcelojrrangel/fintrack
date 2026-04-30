using FinTrack.Application.Common.Abstractions;

namespace FinTrack.Backend.UnitTests.Testing;

internal sealed class NoOpDatabaseInitializer : IDatabaseInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
