namespace FinTrack.Application.Common.Models;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email);
