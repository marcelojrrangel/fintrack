namespace FinTrack.Application.Common.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
