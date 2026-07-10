using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

/// <summary>
/// Only Super Admin / Site Admin can create users (enforced by controller
/// authorization policy — see docs/SECURITY.md RBAC matrix).
/// </summary>
public record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    string DisplayName,
    Guid? PrimaryClientOrganizationId,
    IReadOnlyCollection<string> RoleNames,
    IReadOnlyCollection<Guid> ClientOrganizationIds) : IRequest<UserDto>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        // Mirrors OWASP ASVS password-strength guidance; replaces the legacy MD5+3DES scheme entirely.
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(128);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RoleNames).NotEmpty();
    }
}

public class CreateUserCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var usernameTaken = await db.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (usernameTaken)
        {
            throw new ConflictException($"Username '{request.Username}' is already in use.");
        }

        var roles = await db.Roles.Where(r => request.RoleNames.Contains(r.Name)).ToListAsync(cancellationToken);
        if (roles.Count != request.RoleNames.Count)
        {
            throw new NotFoundException("Role", string.Join(",", request.RoleNames));
        }

        var user = new User
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Username = request.Username,
            Email = request.Email,
            DisplayName = request.DisplayName,
            PasswordHash = passwordHasher.Hash(request.Password),
            PrimaryClientOrganizationId = request.PrimaryClientOrganizationId,
            IsActive = true,
        };

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, User = user, RoleId = role.Id, Role = role });
        }

        foreach (var orgId in request.ClientOrganizationIds.Distinct())
        {
            user.UserOrganizations.Add(new UserOrganization { UserId = user.Id, User = user, ClientOrganizationId = orgId });
        }

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id, user.Username, user.Email, user.DisplayName, user.IsActive, user.IsLocked,
            roles.Select(r => r.Name).ToArray(), request.ClientOrganizationIds, user.LastLoginAtUtc);
    }
}
