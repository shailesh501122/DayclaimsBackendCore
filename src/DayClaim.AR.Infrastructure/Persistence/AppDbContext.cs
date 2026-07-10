using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.ClaimEntries;
using DayClaim.AR.Domain.Identity;
using DayClaim.AR.Domain.Importer;
using DayClaim.AR.Domain.Masters;
using DayClaim.AR.Domain.Notes;
using DayClaim.AR.Domain.RuleEngine;
using DayClaim.AR.Domain.Wfm;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Infrastructure.Persistence;

/// <summary>
/// Single physical database for local/dev Docker Compose. Table names are
/// prefixed by module (users_*, importer_*, rd_* for report data, rule_*,
/// wfm_*, notes_*) so the same layout maps cleanly onto the target
/// database-per-service split described in docs/ARCHITECTURE.md — splitting
/// later is a deployment change, not a schema rewrite.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<ClientOrganization> ClientOrganizations => Set<ClientOrganization>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<ImporterConfig> ImporterConfigs => Set<ImporterConfig>();
    public DbSet<ImporterFieldMapping> ImporterFieldMappings => Set<ImporterFieldMapping>();
    public DbSet<DataImportSourceReference> DataImportSourceReferences => Set<DataImportSourceReference>();

    public DbSet<PatientMaster> PatientMasters => Set<PatientMaster>();
    public DbSet<EnterprisePatientIndex> EnterprisePatientIndexes => Set<EnterprisePatientIndex>();
    public DbSet<PayerMaster> PayerMasters => Set<PayerMaster>();
    public DbSet<CptMaster> CptMasters => Set<CptMaster>();
    public DbSet<PracticeMaster> PracticeMasters => Set<PracticeMaster>();
    public DbSet<StateMaster> StateMasters => Set<StateMaster>();
    public DbSet<ProviderMaster> ProviderMasters => Set<ProviderMaster>();

    public DbSet<RcmReportEntry> RcmReportEntries => Set<RcmReportEntry>();
    public DbSet<RcmReportDataEntrySyncRequest> RcmReportDataEntrySyncRequests => Set<RcmReportDataEntrySyncRequest>();
    public DbSet<AgeingSlaHistory> AgeingSlaHistories => Set<AgeingSlaHistory>();

    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<RuleExecutionRun> RuleExecutionRuns => Set<RuleExecutionRun>();
    public DbSet<RuleExecutionResult> RuleExecutionResults => Set<RuleExecutionResult>();

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<WfmAllocationRule> WfmAllocationRules => Set<WfmAllocationRule>();
    public DbSet<Allocation> Allocations => Set<Allocation>();

    public DbSet<ScenarioMaster> ScenarioMasters => Set<ScenarioMaster>();
    public DbSet<ClaimNote> ClaimNotes => Set<ClaimNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Soft-delete filter applied uniformly — nothing in the AR domain is hard-deleted.
        modelBuilder.Entity<ClientOrganization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ImporterConfig>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ImporterFieldMapping>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DataImportSourceReference>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PatientMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EnterprisePatientIndex>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PayerMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CptMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PracticeMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<StateMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProviderMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RcmReportEntry>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Rule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Team>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WfmAllocationRule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ScenarioMaster>().HasQueryFilter(e => !e.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
