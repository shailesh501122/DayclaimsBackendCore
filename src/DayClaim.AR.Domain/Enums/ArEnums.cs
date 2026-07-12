namespace DayClaim.AR.Domain.Enums;

/// <summary>WFM/Rule Engine work bucket a claim is classified into (per the AR workflow: Setup > Rule Engine Setup).</summary>
public enum ClaimBucket
{
    Workable = 1,
    NonWorkable = 2,
    Calling = 3,
    NonCalling = 4,
    Review = 5,
}

/// <summary>Follow-up priority queue (P1 highest .. P7 lowest), plus the two out-of-band lanes.</summary>
public enum ClaimPriority
{
    P1 = 1,
    P2 = 2,
    P3 = 3,
    P4 = 4,
    P5 = 5,
    P6 = 6,
    P7 = 7,
    SpecialProject = 8,
    ManualAssignment = 9,
}

public enum ImportSourceType
{
    Sftp = 1,
    Scp = 2,
    RestApi = 3,
    Website = 4,
}

public enum ImportDataFormat
{
    Csv = 1,
    Xlsx = 2,
    Json = 3,
    Xml = 4,
}

/// <summary>RCM report families the Data Importer ingests (deck slide 23).</summary>
public enum RcmReportType
{
    Ageing = 1,
    Denials = 2,
    Charges = 3,
    Payments = 4,
    Adjustments = 5,
    Tasking = 6,
}

public enum ImportScheduleTrigger
{
    Scheduled = 1,
    Reactive = 2,
}

/// <summary>Per-field classification used when segregating imported claim data (deck slide 24).</summary>
public enum FieldClassification
{
    Standard = 1,
    CustomerSpecific = 2,
    CustomDefined = 3,
    StatusStoring = 4,
}

/// <summary>Data-quality lifecycle status for an imported claim entry (deck slide 27).</summary>
public enum DataQualityStatus
{
    AutoApproved = 1,
    ApprovalAwaited = 2,
    ManuallyApproved = 3,
    ManuallyRejected = 4,
    Rejected = 5,
    ReOpened = 6,
    Closed = 7,
    Returned = 8,
}

/// <summary>Scope a Rule Engine rule applies at (deck slide 6: Global / Internal / Client / Payer specific rules).</summary>
public enum RuleScope
{
    Global = 1,
    Internal = 2,
    Client = 3,
    Payer = 4,
}

public enum AllocationType
{
    Automatic = 1,
    Manual = 2,
    SpecialProject = 3,
}

public enum InternalRoleName
{
    Admin = 1,
    Manager = 2,
    TeamLeader = 3,
    User = 4,
}

public enum ClientOrgSize
{
    Small = 1,
    Medium = 2,
    Large = 3,
    ExtraLarge = 4,
}
