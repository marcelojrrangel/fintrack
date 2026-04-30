using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Exceptions;

namespace FinTrack.Application.Common.Helpers;

public static class CurrentUserAccessor
{
    public static Guid GetRequiredUserId(ICurrentUserService currentUserService)
    {
        if (currentUserService.UserId is Guid userId)
        {
            return userId;
        }

        throw new UnauthorizedException("The X-User-Id header is required.");
    }
}
