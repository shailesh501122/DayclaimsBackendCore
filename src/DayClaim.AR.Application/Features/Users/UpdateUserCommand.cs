using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

/// <summary>
/// Only Admin/Manager may call this (see PolicyNames.UserManagement). The
/// handler additionally enforces that a Manager can neither grant Admin/
/// Manager roles nor edit an account that currently holds either — only an
/// Admin can create or modify another Admin/Manager, preventing a Manager
/// from escalating their own or a peer's privileges.
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> RoleNames,
    IReadOnlyCollection<Guid> ClientOrganizationIds) : IRequest<UserDto>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RoleNames).NotEmpty();
    }
}

public class UpdateUserCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    private static readonly HashSet<string> ElevatedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "Manager" };

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOrganizations)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        var isRequestingElevatedRole = request.RoleNames.Any(ElevatedRoles.Contains);
        var targetIsCurrentlyElevated = user.UserRoles.Any(ur => ElevatedRoles.Contains(ur.Role.Name));
        if (!currentUser.IsInRole("Admin") && (isRequestingElevatedRole || targetIsCurrentlyElevated))
        {
            throw new ForbiddenAccessException("Only an Admin can assign the Admin/Manager role or modify an Admin/Manager account.");
        }

        var roles = await db.Roles.Where(r => request.RoleNames.Contains(r.Name)).ToListAsync(cancellationToken);
        if (roles.Count != request.RoleNames.Count)
        {
            throw new NotFoundException("Role", string.Join(",", request.RoleNames));
        }

        user.Email = request.Email;
        user.DisplayName = request.DisplayName;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        user.UpdatedByUserId = currentUser.UserId;

        user.UserRoles.Clear();
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, User = user, RoleId = role.Id, Role = role });
        }

        user.UserOrganizations.Clear();
        foreach (var orgId in request.ClientOrganizationIds.Distinct())
        {
            user.UserOrganizations.Add(new UserOrganization { UserId = user.Id, User = user, ClientOrganizationId = orgId });
        }

        await db.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id, user.Username, user.Email, user.DisplayName, user.IsActive, user.IsLocked,
            roles.Select(r => r.Name).ToArray(), request.ClientOrganizationIds, user.LastLoginAtUtc);
    }
}
