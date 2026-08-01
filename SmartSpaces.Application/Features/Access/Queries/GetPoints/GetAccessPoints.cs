using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Formatting;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Access.Queries.GetPoints;

public record GetAccessPointsQuery() : IRequest<IReadOnlyList<AccessPointDto>>;

public record AccessPointDto(
    Guid Id,
    string Name,
    string Building,
    int ScansToday,
    string LastValidation,
    int NetworkPing,
    string Status
);

public class GetAccessPointsQueryHandler : IRequestHandler<GetAccessPointsQuery, IReadOnlyList<AccessPointDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAccessPointsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AccessPointDto>> Handle(GetAccessPointsQuery request, CancellationToken cancellationToken)
    {
        var points = await _context.AccessPoints
            .AsNoTracking()
            .OrderBy(p => p.Building)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        if (points.Count == 0)
        {
            return [];
        }

        var deviceIds = points.Select(p => p.DeviceId).ToList();
        var todayUtc = DateTime.UtcNow.Date;

        // Los contadores no se guardan en la tabla de puntos: se derivan del log de accesos,
        // que es la única fuente que no se puede desincronizar de lo que realmente pasó.
        var scansToday = await _context.AccessLogs
            .AsNoTracking()
            .Where(a => deviceIds.Contains(a.DeviceId) && a.Timestamp >= todayUtc)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Total, cancellationToken);

        var lastValidations = await _context.AccessLogs
            .AsNoTracking()
            .Where(a => deviceIds.Contains(a.DeviceId))
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Last = g.Max(a => a.Timestamp) })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Last, cancellationToken);

        return points
            .Select(p => new AccessPointDto(
                p.Id,
                p.Name,
                p.Building,
                scansToday.TryGetValue(p.DeviceId, out var total) ? total : 0,
                DisplayFormat.RelativeTime(
                    lastValidations.TryGetValue(p.DeviceId, out var last) ? last : null,
                    emptyLabel: "Sin validaciones"),
                p.NetworkPingMs,
                p.Status))
            .ToList();
    }
}
