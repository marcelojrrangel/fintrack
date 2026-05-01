using FinTrack.Application.Common.Abstractions;
using FinTrack.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Application.Features.Users.Queries;

public sealed record GetUsersQuery(string? Search = null) : IRequest<IReadOnlyCollection<UserDto>>;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyCollection<UserDto>>
{
    private readonly IFinTrackDbContext _dbContext;

    public GetUsersQueryHandler(IFinTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(user =>
                user.FullName.ToLower().Contains(search) ||
                user.Email.ToLower().Contains(search));
        }

        return await query
            .OrderBy(user => user.FullName)
            .Select(user => new UserDto(user.Id, user.FullName, user.Email))
            .ToListAsync(cancellationToken);
    }
}
