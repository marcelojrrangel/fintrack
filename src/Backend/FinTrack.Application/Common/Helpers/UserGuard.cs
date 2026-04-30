using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Common.Helpers;

public static class UserGuard
{
    public static async Task EnsureExistsAsync(
        IFinTrackDbContext dbContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException($"User '{userId}' was not found.");
        }
    }
}
