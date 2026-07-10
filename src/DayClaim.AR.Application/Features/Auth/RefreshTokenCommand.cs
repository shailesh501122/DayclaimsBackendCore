using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Auth;

/// <summary>Rotates a refresh token: the old one is revoked and replaced, never reused (mitigates token replay).</summary>
public record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResultDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null || stored.RevokedAtUtc is not null || stored.ExpiresAtUtc < DateTimeOffset.UtcNow)
        {
            throw new ForbiddenAccessException("Refresh token is invalid or expired.");
        }

        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOrganizations)
            .FirstOrDefaultAsync(u => u.Id == stored.UserId && u.IsActive && !u.IsLocked && !u.IsDeleted, cancellationToken)
            ?? throw new ForbiddenAccessException("Account is no longer active.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var orgIds = user.UserOrganizations.Select(o => o.ClientOrganizationId).ToArray();
        var tokens = tokenService.IssueTokens(user, roles, orgIds);

        var replacement = new Domain.Identity.RefreshToken
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(tokens.RefreshToken),
            Jti = tokens.Jti,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress,
        };
        db.RefreshTokens.Add(replacement);

        stored.RevokedAtUtc = DateTimeOffset.UtcNow;
        stored.ReplacedByTokenId = replacement.Id;

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAtUtc, user.Id, user.DisplayName, roles);
    }
}
