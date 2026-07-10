using DayClaim.AR.Domain.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayClaim.AR.Infrastructure.Persistence.Configurations;

public class ScenarioMasterConfig : IEntityTypeConfiguration<ScenarioMaster>
{
    public void Configure(EntityTypeBuilder<ScenarioMaster> builder)
    {
        builder.ToTable("notes_scenario_master");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NoteTemplate).HasMaxLength(4000);
        builder.HasIndex(x => x.ClientOrganizationId);
    }
}

public class ClaimNoteConfig : IEntityTypeConfiguration<ClaimNote>
{
    public void Configure(EntityTypeBuilder<ClaimNote> builder)
    {
        builder.ToTable("notes_claim_notes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NoteText).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => x.RcmReportEntryId);
    }
}
