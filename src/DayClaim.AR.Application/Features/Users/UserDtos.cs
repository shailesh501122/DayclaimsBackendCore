namespace DayClaim.AR.Application.Features.Users;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string DisplayName,
    bool IsActive,
    bool IsLocked,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<Guid> ClientOrganizationIds,
    DateTimeOffset? LastLoginAtUtc);
