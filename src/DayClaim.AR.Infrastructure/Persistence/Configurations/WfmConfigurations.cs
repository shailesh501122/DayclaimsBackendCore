using DayClaim.AR.Domain.Wfm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayClaim.AR.Infrastructure.Persistence.Configurations;

public class TeamConfig : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("wfm_teams");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasMany(x => x.Members).WithOne(m => m.Team).HasForeignKey(m => m.TeamId);
    }
}

public class TeamMemberConfig : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("wfm_team_members");
        builder.HasKey(x => new { x.TeamId, x.UserId });
    }
}

public class WfmAllocationRuleConfig : IEntityTypeConfiguration<WfmAllocationRule>
{
    public void Configure(EntityTypeBuilder<WfmAllocationRule> builder)
    {
        builder.ToTable("wfm_allocation_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TargetBucket).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TargetPriority).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.AllocationType).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.ClientOrganizationId, x.IsActive });
    }
}

public class AllocationConfig : IEntityTypeConfiguration<Allocation>
{
    public void Configure(EntityTypeBuilder<Allocation> builder)
    {
        builder.ToTable("wfm_allocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AllocationType).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.RcmReportEntryId);
        builder.HasIndex(x => x.AssignedToUserId);
    }
}
