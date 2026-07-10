using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Application.Features.ImporterConfig;

public record FieldMappingDto(
    Guid Id,
    string SourceColumnName,
    string TargetFieldName,
    FieldClassification Classification,
    bool IsMandatory,
    bool IsUniquePrimaryIdentifier,
    bool IsUniqueSecondaryIdentifier,
    bool ContainsPhi);

public record ImporterConfigDto(
    Guid Id,
    Guid ClientOrganizationId,
    RcmReportType RcmReportType,
    ImportSourceType SourceType,
    ImportDataFormat DataFormat,
    ImportScheduleTrigger ScheduleTrigger,
    string? ImportFrequencyCron,
    bool IsActive,
    IReadOnlyCollection<FieldMappingDto> FieldMappings);
