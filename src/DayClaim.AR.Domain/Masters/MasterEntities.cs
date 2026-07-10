using DayClaim.AR.Domain.Common;

namespace DayClaim.AR.Domain.Masters;

/// <summary>
/// Shared base for master/reference data that ETL can create on the fly
/// (deck slide 26: "Upsert masters: payers, CPT, practices, states... mark
/// as approval_pending if new").
/// </summary>
public abstract class MasterEntityBase : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsApprovalPending { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PayerMaster : MasterEntityBase
{
}

public class CptMaster : MasterEntityBase
{
    public string? Category { get; set; }
}

public class PracticeMaster : MasterEntityBase
{
}

public class StateMaster : MasterEntityBase
{
}

public class ProviderMaster : MasterEntityBase
{
    public string? Specialty { get; set; }
}
