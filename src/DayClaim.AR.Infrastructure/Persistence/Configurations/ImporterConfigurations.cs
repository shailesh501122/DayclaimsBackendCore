using DayClaim.AR.Domain.Importer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayClaim.AR.Infrastructure.Persistence.Configurations;

public class ImporterConfigConfig : IEntityTypeConfiguration<ImporterConfig>
{
    public void Configure(EntityTypeBuilder<ImporterConfig> builder)
    {
        builder.ToTable("importer_configs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RcmReportType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.DataFormat).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ScheduleTrigger).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.ClientOrganizationId, x.RcmReportType });
        builder.HasMany(x => x.FieldMappings).WithOne().HasForeignKey(m => m.ImporterConfigId);
    }
}

public class ImporterFieldMappingConfig : IEntityTypeConfiguration<ImporterFieldMapping>
{
    public void Configure(EntityTypeBuilder<ImporterFieldMapping> builder)
    {
        builder.ToTable("importer_field_mappings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceColumnName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.TargetFieldName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Classification).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.ImporterConfigId);
    }
}

public class DataImportSourceReferenceConfig : IEntityTypeConfiguration<DataImportSourceReference>
{
    public void Configure(EntityTypeBuilder<DataImportSourceReference> builder)
    {
        builder.ToTable("importer_data_import_source_references");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ObjectStorageKey).HasMaxLength(1000);
        builder.Property(x => x.Sha256Checksum).HasMaxLength(64);
        builder.Property(x => x.OverallStatus).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.ClientOrganizationId, x.ReceivedAtUtc });
    }
}
