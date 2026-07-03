using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Access.Queries.GetLog;

public record GetAccessLogQuery(int Limit = 50) : IRequest<IReadOnlyList<AccessLogDto>>;

public record AccessLogDto(
    Guid Id,
    Guid UserId,
    string? UserName,
    string DeviceId,
    string Direction,
    bool Granted,
    DateTime Timestamp
);

public class GetAccessLogQueryHandler : IRequestHandler<GetAccessLogQuery, IReadOnlyList<AccessLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAccessLogQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AccessLogDto>> Handle(GetAccessLogQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit is > 0 and <= 200 ? request.Limit : 50;

        return await _context.AccessLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new AccessLogDto(
                a.Id,
                a.UserId,
                a.User != null ? a.User.Name : null,
                a.DeviceId,
                a.Direction,
                a.Granted,
                a.Timestamp
            ))
            .ToListAsync(cancellationToken);
    }
}
