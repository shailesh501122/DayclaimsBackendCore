using DayClaim.AR.Domain.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayClaim.AR.Infrastructure.Persistence.Configurations;

public class PayerMasterConfig : IEntityTypeConfiguration<PayerMaster>
{
    public void Configure(EntityTypeBuilder<PayerMaster> builder)
    {
        builder.ToTable("masters_payers");
        ConfigureMaster(builder);
    }

    internal static void ConfigureMaster<T>(EntityTypeBuilder<T> builder) where T : MasterEntityBase
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => x.Name);
    }
}

public class CptMasterConfig : IEntityTypeConfiguration<CptMaster>
{
    public void Configure(EntityTypeBuilder<CptMaster> builder)
    {
        builder.ToTable("masters_cpt_codes");
        PayerMasterConfig.ConfigureMaster(builder);
    }
}

public class PracticeMasterConfig : IEntityTypeConfiguration<PracticeMaster>
{
    public void Configure(EntityTypeBuilder<PracticeMaster> builder)
    {
        builder.ToTable("masters_practices");
        PayerMasterConfig.ConfigureMaster(builder);
    }
}

public class StateMasterConfig : IEntityTypeConfiguration<StateMaster>
{
    public void Configure(EntityTypeBuilder<StateMaster> builder)
    {
        builder.ToTable("masters_states");
        PayerMasterConfig.ConfigureMaster(builder);
    }
}

public class ProviderMasterConfig : IEntityTypeConfiguration<ProviderMaster>
{
    public void Configure(EntityTypeBuilder<ProviderMaster> builder)
    {
        builder.ToTable("masters_providers");
        PayerMasterConfig.ConfigureMaster(builder);
    }
}

public class PatientMasterConfig : IEntityTypeConfiguration<PatientMaster>
{
    public void Configure(EntityTypeBuilder<PatientMaster> builder)
    {
        builder.ToTable("patient_master");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.AccountNumber, o =>
        {
            o.Property(p => p.Ciphertext).HasColumnName("account_number_ciphertext").HasMaxLength(2000);
            o.Property(p => p.BlindIndex).HasColumnName("account_number_blind_index").HasMaxLength(128);
            o.HasIndex(p => p.BlindIndex);
        });
        builder.OwnsOne(x => x.FullName, o =>
        {
            o.Property(p => p.Ciphertext).HasColumnName("full_name_ciphertext").HasMaxLength(2000);
            o.Property(p => p.BlindIndex).HasColumnName("full_name_blind_index").HasMaxLength(128);
        });
        builder.OwnsOne(x => x.DateOfBirth, o =>
        {
            o.Property(p => p.Ciphertext).HasColumnName("dob_ciphertext").HasMaxLength(2000);
            o.Property(p => p.BlindIndex).HasColumnName("dob_blind_index").HasMaxLength(128);
        });

        builder.HasIndex(x => x.ClientOrganizationId);
    }
}

public class EnterprisePatientIndexConfig : IEntityTypeConfiguration<EnterprisePatientIndex>
{
    public void Configure(EntityTypeBuilder<EnterprisePatientIndex> builder)
    {
        builder.ToTable("rd_enterprise_patient_index");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EncounterNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.ClientOrganizationId, x.PatientMasterId, x.EncounterNumber });
    }
}
