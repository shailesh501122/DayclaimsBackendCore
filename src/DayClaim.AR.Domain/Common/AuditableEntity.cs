namespace DayClaim.AR.Domain.Common;

/// <summary>
/// Base type for every aggregate. UUIDv7 primary keys are generated in the
/// application layer (see IdGenerator) rather than by the database, matching
/// the "microservice-generated UUIDv7" decision in the platform's modernization design.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>Soft delete flag — nothing in the AR domain is hard-deleted (audit/history requirements).</summary>
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
}

/// <summary>Marker for entities that belong to exactly one client organization ("per-client" schema/data scoping).</summary>
public interface IClientScoped
{
    Guid ClientOrganizationId { get; set; }
}
