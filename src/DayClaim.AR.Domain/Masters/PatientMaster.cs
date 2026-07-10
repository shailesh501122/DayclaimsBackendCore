using DayClaim.AR.Domain.Common;

namespace DayClaim.AR.Domain.Masters;

/// <summary>
/// Patient identity, de-duplicated on name + account number + client (+ DOB)
/// per deck slide 26. Name and DOB are PHI and stored via <see cref="EncryptedValue"/>;
/// AccountNumber is stored encrypted too since it can identify a patient, but
/// keeps a blind index so ETL can look up "does this patient already exist".
/// </summary>
public class PatientMaster : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public EncryptedValue AccountNumber { get; set; } = EncryptedValue.Empty;
    public EncryptedValue FullName { get; set; } = EncryptedValue.Empty;
    public EncryptedValue? DateOfBirth { get; set; }
}

/// <summary>
/// Enterprise Patient Index: the encounter-level fan-out of a patient across
/// payer/location/provider/practice/CPT/financial-class (deck slide 26).
/// </summary>
public class EnterprisePatientIndex : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public Guid PatientMasterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public Guid? PayerMasterId { get; set; }
    public string? Location { get; set; }
    public Guid? ProviderMasterId { get; set; }
    public Guid? PracticeMasterId { get; set; }
    public string? CptCode { get; set; }
    public string? FinancialClass { get; set; }
}
