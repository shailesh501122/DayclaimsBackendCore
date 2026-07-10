using DayClaim.AR.Domain.RuleEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayClaim.AR.Infrastructure.Persistence.Configurations;

public class RuleConfig : IEntityTypeConfiguration<Rule>
{
    public void Configure(EntityTypeBuilder<Rule> builder)
    {
        builder.ToTable("rule_engine_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Scope).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ConditionExpression).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ResultBucket).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ResultPriority).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.ClientOrganizationId, x.IsActive });
    }
}

public class RuleExecutionRunConfig : IEntityTypeConfiguration<RuleExecutionRun>
{
    public void Configure(EntityTypeBuilder<RuleExecutionRun> builder)
    {
        builder.ToTable("rule_engine_execution_runs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(30);
        builder.HasIndex(x => new { x.ClientOrganizationId, x.StartedAtUtc });
    }
}

public class RuleExecutionResultConfig : IEntityTypeConfiguration<RuleExecutionResult>
{
    public void Configure(EntityTypeBuilder<RuleExecutionResult> builder)
    {
        builder.ToTable("rule_engine_execution_results");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Bucket).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.RuleExecutionRunId);
    }
}
