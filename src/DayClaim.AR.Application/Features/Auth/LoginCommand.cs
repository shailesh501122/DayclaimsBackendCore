using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Auth;

/// <summary>
/// Central login (deck slide 22: "Single user store with JWT-based SSO
/// across AR, EVBV & Coding"). Account lockout after repeated failures
/// replaces the legacy scheme's lack of brute-force protection (OWASP A07).
/// </summary>
public record LoginCommand(string Username, string Password, string? IpAddress) : IRequest<AuthResultDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}

public class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<LoginCommand, AuthResultDto>
{
    private const int MaxFailedAttempts = 5;

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOrganizations)
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted, cancellationToken);

        // Same generic failure message whether the user exists or the password is
        // wrong — avoids leaking which one is invalid (OWASP A07 username enumeration).
        var invalidCredentials = new ForbiddenAccessException("Invalid username or password.");

        if (user is null)
        {
            throw invalidCredentials;
        }

        if (user.IsLocked)
        {
            throw new ForbiddenAccessException("This account is locked. Contact an administrator.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenAccessException("This account is inactive.");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedAttempts)
            {
                user.IsLocked = true;
            }
            await db.SaveChangesAsync(cancellationToken);
            throw invalidCredentials;
        }

        user.FailedLoginCount = 0;
        user.LastLoginAtUtc = DateTimeOffset.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var orgIds = user.UserOrganizations.Select(o => o.ClientOrganizationId).ToArray();

        var tokens = tokenService.IssueTokens(user, roles, orgIds);

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Domain.Common.IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(tokens.RefreshToken),
            Jti = tokens.Jti,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAtUtc, user.Id, user.DisplayName, roles);
    }
}
