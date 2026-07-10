using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.ClaimEntries;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;
using DayClaim.AR.Domain.Masters;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.ImporterStorage;

/// <summary>
/// Simplified stand-in for the Glue-driven, stream-per-RCM-report ETL
/// pipeline in the target architecture (deck slides 25-27). It performs the
/// same logical steps synchronously and in-process:
///   1. Record the source file (DataImportSourceReference).
///   2. Upsert PatientMaster + EnterprisePatientIndex per row.
///   3. Upsert referenced masters (payer), flagging brand-new ones as
///      approval-pending rather than auto-trusting ETL-discovered data.
///   4. Classify each row's fields per the importer's field mapping into
///      standard/customer-specific/custom-defined/status-storing/unmapped
///      JSON sections, run basic data-quality checks, and set the resulting
///      <see cref="DataQualityStatus"/>.
///   5. Publish one "claim imported" integration event per entry (stands in
///      for the per-RCM-report stream) so Rule Engine/WFM can react later.
/// A production build replaces steps 1-5 with actual AWS Glue jobs, Valkey/
/// Kafka streams and async consumers — see docs/ARCHITECTURE.md.
/// </summary>
public record IngestFileCommand(
    Guid ClientOrganizationId,
    Guid ImporterConfigId,
    string FileName,
    IReadOnlyCollection<IReadOnlyDictionary<string, string>> Rows) : IRequest<IngestResultDto>;

public record IngestResultDto(
    Guid SourceReferenceId,
    int RowsReceived,
    int AutoApproved,
    int ApprovalAwaited,
    int Rejected);

public class IngestFileCommandValidator : AbstractValidator<IngestFileCommand>
{
    public IngestFileCommandValidator()
    {
        RuleFor(x => x.ClientOrganizationId).NotEmpty();
        RuleFor(x => x.ImporterConfigId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.Rows).NotEmpty();
    }
}

public class IngestFileCommandHandler(
    IApplicationDbContext db,
    IFieldEncryptionService encryption,
    IEventPublisher events,
    IDateTimeProvider clock) : IRequestHandler<IngestFileCommand, IngestResultDto>
{
    public async Task<IngestResultDto> Handle(IngestFileCommand request, CancellationToken cancellationToken)
    {
        var config = await db.ImporterConfigs
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.Id == request.ImporterConfigId && c.ClientOrganizationId == request.ClientOrganizationId, cancellationToken)
            ?? throw new NotFoundException("ImporterConfig", request.ImporterConfigId);

        var rawBytes = Encoding.UTF8.GetBytes(string.Join('\n', request.Rows.Select(r => string.Join(',', r.Values))));
        var checksum = Convert.ToHexString(SHA256.HashData(rawBytes));

        var sourceRef = new Domain.Importer.DataImportSourceReference
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = clock.UtcNow,
            ClientOrganizationId = request.ClientOrganizationId,
            ImporterConfigId = config.Id,
            FileName = request.FileName,
            ObjectStorageKey = $"imports/{request.ClientOrganizationId}/{config.RcmReportType}/{request.FileName}",
            Sha256Checksum = checksum,
            ReceivedAtUtc = clock.UtcNow,
            RowCount = request.Rows.Count,
            OverallStatus = DataQualityStatus.AutoApproved,
        };
        db.DataImportSourceReferences.Add(sourceRef);

        int autoApproved = 0, approvalAwaited = 0, rejected = 0;

        foreach (var row in request.Rows)
        {
            var mandatoryMissing = config.FieldMappings
                .Where(m => m.IsMandatory)
                .Any(m => !row.TryGetValue(m.SourceColumnName, out var v) || string.IsNullOrWhiteSpace(v));

            var patient = await ResolvePatientAsync(request.ClientOrganizationId, row, cancellationToken);
            var (epi, newMasterCreated) = await ResolveEnterprisePatientIndexAsync(request.ClientOrganizationId, patient.Id, row, cancellationToken);

            var sections = ClassifyFields(config.FieldMappings, row);
            var status = mandatoryMissing ? DataQualityStatus.Rejected
                : newMasterCreated ? DataQualityStatus.ApprovalAwaited
                : DataQualityStatus.AutoApproved;

            var entry = new RcmReportEntry
            {
                Id = IdGenerator.NewId(),
                CreatedAtUtc = clock.UtcNow,
                ClientOrganizationId = request.ClientOrganizationId,
                RcmReportType = config.RcmReportType,
                DataImportSourceReferenceId = sourceRef.Id,
                PatientMasterId = patient.Id,
                EnterprisePatientIndexId = epi.Id,
                StandardFieldsJson = sections.Standard,
                CustomerSpecificFieldsJson = sections.CustomerSpecific,
                CustomDefinedFieldsJson = sections.CustomDefined,
                StatusStoringFieldsJson = sections.StatusStoring,
                UnmappedFieldsJson = sections.Unmapped,
                Balance = row.TryGetValue("balance", out var balanceRaw) ? encryption.Encrypt(balanceRaw) : EncryptedValue.Empty,
                Status = status,
                OpenedAtUtc = clock.UtcNow,
            };
            db.RcmReportEntries.Add(entry);

            switch (status)
            {
                case DataQualityStatus.AutoApproved: autoApproved++; break;
                case DataQualityStatus.ApprovalAwaited: approvalAwaited++; break;
                default: rejected++; break;
            }

            if (status is DataQualityStatus.AutoApproved or DataQualityStatus.ReOpened)
            {
                db.RcmReportDataEntrySyncRequests.Add(new RcmReportDataEntrySyncRequest
                {
                    Id = IdGenerator.NewId(),
                    CreatedAtUtc = clock.UtcNow,
                    RcmReportEntryId = entry.Id,
                    RequestedAtUtc = clock.UtcNow,
                    Status = "pending",
                });
            }

            await events.PublishAsync(new ClaimImportedEvent(entry.Id, request.ClientOrganizationId, config.RcmReportType, status), cancellationToken);
        }

        sourceRef.OverallStatus = rejected == request.Rows.Count ? DataQualityStatus.Rejected : DataQualityStatus.AutoApproved;

        await db.SaveChangesAsync(cancellationToken);

        return new IngestResultDto(sourceRef.Id, request.Rows.Count, autoApproved, approvalAwaited, rejected);
    }

    private async Task<PatientMaster> ResolvePatientAsync(Guid clientOrgId, IReadOnlyDictionary<string, string> row, CancellationToken ct)
    {
        var accountNumber = row.GetValueOrDefault("account_number", string.Empty);
        var name = row.GetValueOrDefault("patient_name", string.Empty);
        var blindIndex = encryption.ComputeBlindIndex($"{clientOrgId}:{accountNumber}");

        var existing = await db.PatientMasters
            .FirstOrDefaultAsync(p => p.ClientOrganizationId == clientOrgId && p.AccountNumber.BlindIndex == blindIndex, ct);
        if (existing is not null)
        {
            return existing;
        }

        var patient = new PatientMaster
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = clock.UtcNow,
            ClientOrganizationId = clientOrgId,
            AccountNumber = new EncryptedValue { Ciphertext = encryption.Encrypt(accountNumber).Ciphertext, BlindIndex = blindIndex },
            FullName = encryption.Encrypt(name),
            DateOfBirth = row.TryGetValue("dob", out var dob) && !string.IsNullOrWhiteSpace(dob) ? encryption.Encrypt(dob) : null,
        };
        db.PatientMasters.Add(patient);
        return patient;
    }

    private async Task<(EnterprisePatientIndex Epi, bool NewMasterCreated)> ResolveEnterprisePatientIndexAsync(
        Guid clientOrgId, Guid patientId, IReadOnlyDictionary<string, string> row, CancellationToken ct)
    {
        var encounter = row.GetValueOrDefault("encounter_number", Guid.NewGuid().ToString("N"));
        var existing = await db.EnterprisePatientIndexes
            .FirstOrDefaultAsync(e => e.ClientOrganizationId == clientOrgId && e.PatientMasterId == patientId && e.EncounterNumber == encounter, ct);
        if (existing is not null)
        {
            return (existing, false);
        }

        var (payerId, payerIsNew) = await ResolvePayerAsync(row.GetValueOrDefault("payer_name", "Unknown Payer"), row.GetValueOrDefault("payer_code", null!), ct);

        var epi = new EnterprisePatientIndex
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = clock.UtcNow,
            ClientOrganizationId = clientOrgId,
            PatientMasterId = patientId,
            EncounterNumber = encounter,
            PayerMasterId = payerId,
            Location = row.GetValueOrDefault("location"),
            CptCode = row.GetValueOrDefault("cpt_code"),
            FinancialClass = row.GetValueOrDefault("financial_class"),
        };
        db.EnterprisePatientIndexes.Add(epi);
        return (epi, payerIsNew);
    }

    private async Task<(Guid? Id, bool IsNew)> ResolvePayerAsync(string payerName, string? payerCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(payerName))
        {
            return (null, false);
        }

        var existing = await db.PayerMasters.FirstOrDefaultAsync(p => p.Name == payerName, ct);
        if (existing is not null)
        {
            return (existing.Id, false);
        }

        var payer = new PayerMaster
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = clock.UtcNow,
            Name = payerName,
            Code = payerCode ?? payerName[..Math.Min(6, payerName.Length)].ToUpperInvariant(),
            IsApprovalPending = true,
        };
        db.PayerMasters.Add(payer);
        return (payer.Id, true);
    }

    private static FieldSections ClassifyFields(IEnumerable<Domain.Importer.ImporterFieldMapping> mappings, IReadOnlyDictionary<string, string> row)
    {
        var standard = new Dictionary<string, string>();
        var customerSpecific = new Dictionary<string, string>();
        var customDefined = new Dictionary<string, string>();
        var statusStoring = new Dictionary<string, string>();
        var mappedSourceColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in mappings)
        {
            mappedSourceColumns.Add(mapping.SourceColumnName);
            if (!row.TryGetValue(mapping.SourceColumnName, out var value))
            {
                continue;
            }

            var bucket = mapping.Classification switch
            {
                FieldClassification.Standard => standard,
                FieldClassification.CustomerSpecific => customerSpecific,
                FieldClassification.CustomDefined => customDefined,
                FieldClassification.StatusStoring => statusStoring,
                _ => standard,
            };
            bucket[mapping.TargetFieldName] = value;
        }

        var unmapped = row
            .Where(kv => !mappedSourceColumns.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return new FieldSections(
            JsonSerializer.Serialize(standard),
            JsonSerializer.Serialize(customerSpecific),
            JsonSerializer.Serialize(customDefined),
            JsonSerializer.Serialize(statusStoring),
            JsonSerializer.Serialize(unmapped));
    }

    private record FieldSections(
        string Standard,
        string CustomerSpecific,
        string CustomDefined,
        string StatusStoring,
        string Unmapped);
}

/// <summary>Integration event stand-in for the per-RCM-report stream message (deck slide 25).</summary>
public record ClaimImportedEvent(Guid RcmReportEntryId, Guid ClientOrganizationId, RcmReportType RcmReportType, DataQualityStatus Status);
