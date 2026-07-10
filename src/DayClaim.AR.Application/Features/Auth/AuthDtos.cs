namespace DayClaim.AR.Application.Features.Auth;

public record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    Guid UserId,
    string DisplayName,
    IReadOnlyCollection<string> Roles);
