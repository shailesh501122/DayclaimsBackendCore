using DayClaim.AR.Domain.ClaimEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayClaim.AR.Infrastructure.Persistence.Configurations;

public class RcmReportEntryConfig : IEntityTypeConfiguration<RcmReportEntry>
{
    public void Configure(EntityTypeBuilder<RcmReportEntry> builder)
    {
        // Write-model table (event-sourced: insert + select only — deck slide 26).
        builder.ToTable("rd_rcm_reports_data_entries_initial");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RcmReportType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Bucket).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(30);

        builder.Property(x => x.StandardFieldsJson).HasColumnName("standard_fields").HasColumnType("jsonb");
        builder.Property(x => x.CustomerSpecificFieldsJson).HasColumnName("customer_specific_fields").HasColumnType("jsonb");
        builder.Property(x => x.CustomDefinedFieldsJson).HasColumnName("custom_defined_fields").HasColumnType("jsonb");
        builder.Property(x => x.StatusStoringFieldsJson).HasColumnName("status_storing_fields").HasColumnType("jsonb");
        builder.Property(x => x.UnmappedFieldsJson).HasColumnName("unmapped_fields").HasColumnType("jsonb");

        builder.OwnsOne(x => x.Balance, o =>
        {
            o.Property(p => p.Ciphertext).HasColumnName("balance_ciphertext").HasMaxLength(500);
            o.Property(p => p.BlindIndex).HasColumnName("balance_blind_index").HasMaxLength(128);
        });

        // Partitioned by org (client) x RCM report combination in the target design (deck slide 24);
        // this composite index is the local-Postgres equivalent for query performance.
        builder.HasIndex(x => new { x.ClientOrganizationId, x.RcmReportType, x.Status });
        builder.HasIndex(x => new { x.ClientOrganizationId, x.Bucket, x.Priority });
    }
}

public class RcmReportDataEntrySyncRequestConfig : IEntityTypeConfiguration<RcmReportDataEntrySyncRequest>
{
    public void Configure(EntityTypeBuilder<RcmReportDataEntrySyncRequest> builder)
    {
        builder.ToTable("rd_rcm_reports_data_entry_sync_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(30);
        builder.HasIndex(x => x.RcmReportEntryId);
    }
}

public class AgeingSlaHistoryConfig : IEntityTypeConfiguration<AgeingSlaHistory>
{
    public void Configure(EntityTypeBuilder<AgeingSlaHistory> builder)
    {
        builder.ToTable("rd_ageing_data_sla_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Bucket).HasMaxLength(20);
        builder.HasIndex(x => new { x.RcmReportEntryId, x.SnapshotDate });
    }
}
