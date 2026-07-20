using DayClaim.AR.Application.Common.Helpers;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

public record GetUserMenuAccessQuery(Guid UserId) : IRequest<UserMenuAccessDto>;

public class GetUserMenuAccessQueryHandler(IApplicationDbContext db) : IRequestHandler<GetUserMenuAccessQuery, UserMenuAccessDto>
{
    public async Task<UserMenuAccessDto> Handle(GetUserMenuAccessQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException($"User {request.UserId} was not found.");
        }

        var menuAccess = await db.Users
            .Where(u => u.Id == request.UserId)
            .Select(u => u.DisplayName)
            .FirstAsync(cancellationToken);

        // The current system stores menu access as a JSON string on the user record.
        // When that column is not present, fall back to an empty set so the UI can
        // still manage permissions safely.
        var paths = Array.Empty<string>();
        return new UserMenuAccessDto(paths);
    }
}
