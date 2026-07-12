using DayClaim.AR.Domain.Common;

namespace DayClaim.AR.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string Hash(string plaintextPassword);
    bool Verify(string plaintextPassword, string hash);
}

public record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAtUtc, string Jti);

public interface ITokenService
{
    TokenPair IssueTokens(Domain.Identity.User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<Guid> clientOrganizationIds);
    string HashRefreshToken(string refreshToken);
}

/// <summary>
/// Field-level authenticated encryption (AES-256-GCM) plus a keyed blind
/// index for equality lookups, per the deck's HIPAA design (slide 30).
/// </summary>
public interface IFieldEncryptionService
{
    EncryptedValue Encrypt(string plaintext);
    string Decrypt(EncryptedValue value);
    string ComputeBlindIndex(string plaintext);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>Publishes domain/integration events onto the message broker (AmazonMQ/RabbitMQ in the target design).</summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<Guid> ClientOrganizationIds { get; }
    bool IsInRole(string role);
    /// <summary>True for internal-staff roles (Admin/Manager/Team Leader/User) that are not scoped to a single client org.</summary>
    bool IsInternalStaff { get; }
}

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
