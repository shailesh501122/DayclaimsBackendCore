using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

public record GetUsersQuery(int Page = 1, int PageSize = 20) : IRequest<Common.Models.PagedResult<UserDto>>;

public class GetUsersQueryHandler(IApplicationDbContext db) : IRequestHandler<GetUsersQuery, Common.Models.PagedResult<UserDto>>
{
    public async Task<Common.Models.PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOrganizations)
            .OrderBy(u => u.Username);

        var total = await query.CountAsync(cancellationToken);
        var page = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = page.Select(u => new UserDto(
            u.Id, u.Username, u.Email, u.DisplayName, u.IsActive, u.IsLocked,
            u.UserRoles.Select(ur => ur.Role.Name).ToArray(),
            u.UserOrganizations.Select(o => o.ClientOrganizationId).ToArray(),
            u.LastLoginAtUtc)).ToArray();

        return Common.Models.PagedResult<UserDto>.Create(items, total, request.Page, request.PageSize);
    }
}
