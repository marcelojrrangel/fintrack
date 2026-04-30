using FinTrack.Application.Common.Abstractions;

namespace FinTrack.WebAPI.Services;

public sealed class HttpCurrentUserService : ICurrentUserService
{
    private const string UserIdHeader = "X-User-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var rawValue = _httpContextAccessor.HttpContext?.Request.Headers[UserIdHeader].FirstOrDefault();

            return Guid.TryParse(rawValue, out var userId)
                ? userId
                : null;
        }
    }
}
