using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;
using DayClaim.AR.Domain.Identity;
using DayClaim.AR.Domain.Importer;
using DayClaim.AR.Domain.Notes;
using DayClaim.AR.Domain.RuleEngine;
using DayClaim.AR.Domain.Wfm;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DayClaim.AR.Infrastructure.Persistence.Seed;

/// <summary>
/// Dev/local-only seed data — never runs outside Development (see
/// Program.cs). Mirrors the client/employee names already used in the
/// DayClaim.ai React frontend's mock data so a demo walking both repos
/// together looks like one coherent product.
/// </summary>
public static class DevSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        if (await db.ClientOrganizations.AnyAsync())
        {
            return; // already seeded
        }

        var now = DateTimeOffset.UtcNow;

        var clientOrg = new ClientOrganization
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = now,
            Name = "Austin Heart Group",
            Code = "AHG",
            Size = ClientOrgSize.Medium,
            IsActive = true,
        };
        db.ClientOrganizations.Add(clientOrg);

        var roles = new[] { "Admin", "Manager", "Team Leader", "User" }
            .Select(name => new Role { Id = IdGenerator.NewId(), CreatedAtUtc = now, Name = name })
            .ToArray();
        db.Roles.AddRange(roles);

        Role RoleByName(string name) => roles.First(r => r.Name == name);

        var admin = NewUser("Admin", "admin@dayclaim.ai", "DayClaim Admin", passwordHasher, now, "Admin@123");
        admin.UserRoles.Add(new UserRole { UserId = admin.Id, User = admin, RoleId = RoleByName("Admin").Id });
        // Admin needs no menu-access rows — it always sees every menu (see GetMyMenuAccessQuery).

        var manager = NewUser("sanjay.k", "sanjay.k@dayclaim.ai", "Sanjay Kulkarni", passwordHasher, now);
        manager.UserRoles.Add(new UserRole { UserId = manager.Id, User = manager, RoleId = RoleByName("Manager").Id });
        manager.UserOrganizations.Add(new UserOrganization { UserId = manager.Id, User = manager, ClientOrganizationId = clientOrg.Id });
        Grant(manager, "/dashboard", "/rule-engine", "/importer/importer-setup", "/notes/scenario-master", "/role-management/user-role-management", "/other/login-credentials");

        var teamLeader = NewUser("vikram.rao", "vikram.rao@dayclaim.ai", "Vikram Rao", passwordHasher, now);
        teamLeader.UserRoles.Add(new UserRole { UserId = teamLeader.Id, User = teamLeader, RoleId = RoleByName("Team Leader").Id });
        teamLeader.UserOrganizations.Add(new UserOrganization { UserId = teamLeader.Id, User = teamLeader, ClientOrganizationId = clientOrg.Id });
        Grant(teamLeader, "/dashboard", "/rule-engine");

        var agent1 = NewUser("priya.s", "priya.s@dayclaim.ai", "Priya S", passwordHasher, now);
        agent1.UserRoles.Add(new UserRole { UserId = agent1.Id, User = agent1, RoleId = RoleByName("User").Id });
        agent1.UserOrganizations.Add(new UserOrganization { UserId = agent1.Id, User = agent1, ClientOrganizationId = clientOrg.Id });
        Grant(agent1, "/dashboard");

        var agent2 = NewUser("rahul.m", "rahul.m@dayclaim.ai", "Rahul Menon", passwordHasher, now);
        agent2.UserRoles.Add(new UserRole { UserId = agent2.Id, User = agent2, RoleId = RoleByName("User").Id });
        agent2.UserOrganizations.Add(new UserOrganization { UserId = agent2.Id, User = agent2, ClientOrganizationId = clientOrg.Id });
        Grant(agent2, "/dashboard");

        db.Users.AddRange(admin, manager, teamLeader, agent1, agent2);

        var importerConfig = new ImporterConfig
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = now,
            ClientOrganizationId = clientOrg.Id,
            RcmReportType = RcmReportType.Ageing,
            SourceType = ImportSourceType.Sftp,
            DataFormat = ImportDataFormat.Csv,
            ScheduleTrigger = ImportScheduleTrigger.Scheduled,
            ImportFrequencyCron = "0 6 * * *",
            IsActive = true,
        };
        importerConfig.FieldMappings.Add(Mapping(importerConfig.Id, "account_number", "account_number", FieldClassification.Standard, true, true, false, true, now));
        importerConfig.FieldMappings.Add(Mapping(importerConfig.Id, "patient_name", "patient_name", FieldClassification.Standard, true, false, true, true, now));
        importerConfig.FieldMappings.Add(Mapping(importerConfig.Id, "balance", "balance", FieldClassification.Standard, true, false, false, false, now));
        importerConfig.FieldMappings.Add(Mapping(importerConfig.Id, "payer_name", "payer_name", FieldClassification.Standard, true, false, false, false, now));
        importerConfig.FieldMappings.Add(Mapping(importerConfig.Id, "ageing_bucket", "ageing_bucket", FieldClassification.Standard, false, false, false, false, now));
        db.ImporterConfigs.Add(importerConfig);

        db.Rules.AddRange(
            NewRule("High-balance workable", RuleScope.Global, null, "balance > 5000", ClaimBucket.Workable, ClaimPriority.P1, 1, now),
            NewRule("Standard workable", RuleScope.Global, null, "balance > 100", ClaimBucket.Workable, ClaimPriority.P3, 2, now),
            NewRule("Low-balance non-workable", RuleScope.Global, null, "balance > 0", ClaimBucket.NonWorkable, null, 3, now));

        var team = new Team
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = now,
            ClientOrganizationId = clientOrg.Id,
            Name = "AR Team Alpha",
            TeamLeaderUserId = teamLeader.Id,
        };
        team.Members.Add(new TeamMember { TeamId = team.Id, Team = team, UserId = agent1.Id, IsExperienced = true });
        team.Members.Add(new TeamMember { TeamId = team.Id, Team = team, UserId = agent2.Id, IsExperienced = false });
        db.Teams.Add(team);

        db.WfmAllocationRules.Add(new WfmAllocationRule
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = now,
            ClientOrganizationId = clientOrg.Id,
            TeamId = team.Id,
            TargetBucket = ClaimBucket.Workable,
            TargetPriority = ClaimPriority.P1,
            AllocationType = AllocationType.Automatic,
            EqualDistribution = true,
            IsActive = true,
        });

        db.ScenarioMasters.Add(new ScenarioMaster
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = now,
            ClientOrganizationId = clientOrg.Id,
            Name = "Denied - Timely Filing",
            StatusActionSubActionMapping = "Denied > Appeal > Timely Filing",
            NoteTemplate = "Claim {encounter} denied by {payer}. Appeal due by {followup_date}.",
            DisplayOrder = 1,
            IsActive = true,
        });

        await db.SaveChangesAsync();
    }

    /// <summary>Grants a non-Admin demo user a starter set of visible menus — paths
    /// are the frontend's slugified routes (see dayclaim repo's routes/menuRoutes.js),
    /// an intentional loose coupling since the menu catalog itself lives in the
    /// frontend, not this backend.</summary>
    private static void Grant(User user, params string[] menuPaths)
    {
        foreach (var path in menuPaths)
        {
            user.MenuAccess.Add(new UserMenuAccess { UserId = user.Id, User = user, MenuPath = path });
        }
    }

    private static User NewUser(string username, string email, string displayName, IPasswordHasher hasher, DateTimeOffset now, string password = "admin") => new()
    {
        Id = IdGenerator.NewId(),
        CreatedAtUtc = now,
        Username = username,
        Email = email,
        DisplayName = displayName,
        // Dev-only fixed password so the seeded demo accounts are simple and
        // reproducible for local testing. Never seed a fixed/weak password in a real
        // environment — see docs/SECURITY.md.
        PasswordHash = hasher.Hash(password),
        IsActive = true,
    };

    private static ImporterFieldMapping Mapping(
        Guid configId, string source, string target, FieldClassification classification,
        bool mandatory, bool primaryId, bool secondaryId, bool phi, DateTimeOffset now) => new()
    {
        Id = IdGenerator.NewId(),
        CreatedAtUtc = now,
        ImporterConfigId = configId,
        SourceColumnName = source,
        TargetFieldName = target,
        Classification = classification,
        IsMandatory = mandatory,
        IsUniquePrimaryIdentifier = primaryId,
        IsUniqueSecondaryIdentifier = secondaryId,
        ContainsPhi = phi,
    };

    private static Rule NewRule(
        string name, RuleScope scope, Guid? clientOrgId, string condition,
        ClaimBucket bucket, ClaimPriority? priority, int order, DateTimeOffset now) => new()
    {
        Id = IdGenerator.NewId(),
        CreatedAtUtc = now,
        Name = name,
        Scope = scope,
        ClientOrganizationId = clientOrgId,
        ConditionExpression = condition,
        ResultBucket = bucket,
        ResultPriority = priority,
        EvaluationOrder = order,
        IsActive = true,
    };
}
