using FinTrack.Application.Common.Abstractions;

namespace FinTrack.Backend.UnitTests.Testing;

internal sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; init; }
}
