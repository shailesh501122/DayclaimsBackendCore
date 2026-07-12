using DayClaim.AR.Domain.Identity;
using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

/// <summary>Deactivating blocks login immediately; reactivating also clears
/// any failed-login lockout so an Admin/Manager can undo an account lockout
/// without a separate "unlock" action.</summary>
public record SetUserActiveCommand(Guid Id, bool IsActive) : IRequest<UserDto>;

public class SetUserActiveCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<SetUserActiveCommand, UserDto>
{
    private static readonly HashSet<string> ElevatedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "Manager" };

    public async Task<UserDto> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOrganizations)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        if (!currentUser.IsInRole("Admin") && user.UserRoles.Any(ur => ElevatedRoles.Contains(ur.Role.Name)))
        {
            throw new ForbiddenAccessException("Only an Admin can activate/deactivate an Admin/Manager account.");
        }

        if (user.Id == currentUser.UserId && !request.IsActive)
        {
            throw new ForbiddenAccessException("You cannot deactivate your own account.");
        }

        user.IsActive = request.IsActive;
        if (request.IsActive)
        {
            user.IsLocked = false;
            user.FailedLoginCount = 0;
        }
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        user.UpdatedByUserId = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id, user.Username, user.Email, user.DisplayName, user.IsActive, user.IsLocked,
            user.UserRoles.Select(ur => ur.Role.Name).ToArray(),
            user.UserOrganizations.Select(o => o.ClientOrganizationId).ToArray(),
            user.LastLoginAtUtc);
    }
}
