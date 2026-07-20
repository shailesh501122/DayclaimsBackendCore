using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

public record RoleDto(Guid Id, string Name);
public record GetRolesQuery : IRequest<IReadOnlyCollection<RoleDto>>;

public class GetRolesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetRolesQuery, IReadOnlyCollection<RoleDto>>
{
    public async Task<IReadOnlyCollection<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) =>
        await db.Roles
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(r.Id, r.Name))
            .ToArrayAsync(cancellationToken);
}
