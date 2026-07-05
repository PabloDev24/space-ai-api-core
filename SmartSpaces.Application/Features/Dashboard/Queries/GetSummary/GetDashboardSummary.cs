using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Dashboard.Queries.GetSummary;

public record GetDashboardSummaryQuery() : IRequest<DashboardSummaryDto>;

public record DashboardDevicesDto(int Online, int Offline);

public record DashboardSummaryDto(
    int UsersCount,
    int ActiveSessions,
    int AccessToday,
    int QueriesToday,
    DashboardDevicesDto Devices
);

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var todayUtc = DateTime.UtcNow.Date;

        var usersCount = await _context.Users.CountAsync(cancellationToken);
        var activeSessions = await _context.Sessions.CountAsync(s => s.IsActive, cancellationToken);
        var accessToday = await _context.AccessLogs.CountAsync(a => a.Timestamp >= todayUtc, cancellationToken);
        var queriesToday = await _context.KnowledgeQueries.CountAsync(q => q.CreatedAt >= todayUtc, cancellationToken);

        var onlineDevices = await _context.Devices.CountAsync(d => d.Status == "ONLINE", cancellationToken);
        var offlineDevices = await _context.Devices.CountAsync(d => d.Status != "ONLINE", cancellationToken);
        var devices = new DashboardDevicesDto(Online: onlineDevices, Offline: offlineDevices);

        return new DashboardSummaryDto(usersCount, activeSessions, accessToday, queriesToday, devices);
    }
}
