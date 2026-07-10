using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.ImporterConfig;

public record GetImporterConfigsQuery(Guid ClientOrganizationId) : IRequest<IReadOnlyCollection<ImporterConfigDto>>;

public class GetImporterConfigsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetImporterConfigsQuery, IReadOnlyCollection<ImporterConfigDto>>
{
    public async Task<IReadOnlyCollection<ImporterConfigDto>> Handle(GetImporterConfigsQuery request, CancellationToken cancellationToken)
    {
        var configs = await db.ImporterConfigs
            .Include(c => c.FieldMappings.Where(m => !m.IsDeleted))
            .Where(c => c.ClientOrganizationId == request.ClientOrganizationId && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        return configs.Select(CreateImporterConfigCommandHandler.ToDto).ToArray();
    }
}
