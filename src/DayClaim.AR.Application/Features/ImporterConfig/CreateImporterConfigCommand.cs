using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;
using FluentValidation;
using MediatR;

namespace DayClaim.AR.Application.Features.ImporterConfig;

/// <summary>Deck slide 23: per-client RCM report mapping, deployed/managed centrally.</summary>
public record CreateImporterConfigCommand(
    Guid ClientOrganizationId,
    RcmReportType RcmReportType,
    ImportSourceType SourceType,
    ImportDataFormat DataFormat,
    ImportScheduleTrigger ScheduleTrigger,
    string? ImportFrequencyCron,
    IReadOnlyCollection<CreateFieldMapping> FieldMappings) : IRequest<ImporterConfigDto>;

public record CreateFieldMapping(
    string SourceColumnName,
    string TargetFieldName,
    FieldClassification Classification,
    bool IsMandatory,
    bool IsUniquePrimaryIdentifier,
    bool IsUniqueSecondaryIdentifier,
    bool ContainsPhi);

public class CreateImporterConfigCommandValidator : AbstractValidator<CreateImporterConfigCommand>
{
    public CreateImporterConfigCommandValidator()
    {
        RuleFor(x => x.ClientOrganizationId).NotEmpty();
        RuleFor(x => x.FieldMappings).NotEmpty();
        RuleForEach(x => x.FieldMappings).ChildRules(mapping =>
        {
            mapping.RuleFor(m => m.SourceColumnName).NotEmpty().MaximumLength(128);
            mapping.RuleFor(m => m.TargetFieldName).NotEmpty().MaximumLength(128);
        });
        RuleFor(x => x.ImportFrequencyCron)
            .NotEmpty()
            .When(x => x.ScheduleTrigger == Domain.Enums.ImportScheduleTrigger.Scheduled)
            .WithMessage("A cron expression is required when the import is schedule-triggered.");
    }
}

public class CreateImporterConfigCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateImporterConfigCommand, ImporterConfigDto>
{
    public async Task<ImporterConfigDto> Handle(CreateImporterConfigCommand request, CancellationToken cancellationToken)
    {
        var config = new Domain.Importer.ImporterConfig
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ClientOrganizationId = request.ClientOrganizationId,
            RcmReportType = request.RcmReportType,
            SourceType = request.SourceType,
            DataFormat = request.DataFormat,
            ScheduleTrigger = request.ScheduleTrigger,
            ImportFrequencyCron = request.ImportFrequencyCron,
            IsActive = true,
        };

        foreach (var mapping in request.FieldMappings)
        {
            config.FieldMappings.Add(new Domain.Importer.ImporterFieldMapping
            {
                Id = IdGenerator.NewId(),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                ImporterConfigId = config.Id,
                SourceColumnName = mapping.SourceColumnName,
                TargetFieldName = mapping.TargetFieldName,
                Classification = mapping.Classification,
                IsMandatory = mapping.IsMandatory,
                IsUniquePrimaryIdentifier = mapping.IsUniquePrimaryIdentifier,
                IsUniqueSecondaryIdentifier = mapping.IsUniqueSecondaryIdentifier,
                ContainsPhi = mapping.ContainsPhi,
            });
        }

        db.ImporterConfigs.Add(config);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(config);
    }

    internal static ImporterConfigDto ToDto(Domain.Importer.ImporterConfig config) => new(
        config.Id, config.ClientOrganizationId, config.RcmReportType, config.SourceType, config.DataFormat,
        config.ScheduleTrigger, config.ImportFrequencyCron, config.IsActive,
        config.FieldMappings.Select(m => new FieldMappingDto(
            m.Id, m.SourceColumnName, m.TargetFieldName, m.Classification, m.IsMandatory,
            m.IsUniquePrimaryIdentifier, m.IsUniqueSecondaryIdentifier, m.ContainsPhi)).ToArray());
}
