using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Dashboard.Queries.GetActivity;

public record GetDashboardActivityQuery(int Limit = 15) : IRequest<IReadOnlyList<ActivityItemDto>>;

public record ActivityItemDto(string Type, string Message, DateTime Timestamp);

public class GetDashboardActivityQueryHandler : IRequestHandler<GetDashboardActivityQuery, IReadOnlyList<ActivityItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardActivityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ActivityItemDto>> Handle(GetDashboardActivityQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit is > 0 and <= 100 ? request.Limit : 15;

        var accesses = await _context.AccessLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new ActivityItemDto(
                "ACCESS",
                (a.User != null ? a.User.Name : "Alguien") +
                    (a.Direction == "OUT" ? " registró salida" : " registró entrada"),
                a.Timestamp
            ))
            .ToListAsync(cancellationToken);

        var queries = await _context.KnowledgeQueries
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedAt)
            .Take(limit)
            .Select(q => new ActivityItemDto(
                "QUERY",
                (q.User != null ? q.User.Name : "Alguien") + " preguntó: " + q.Question,
                q.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return accesses
            .Concat(queries)
            .OrderByDescending(i => i.Timestamp)
            .Take(limit)
            .ToList();
    }
}
