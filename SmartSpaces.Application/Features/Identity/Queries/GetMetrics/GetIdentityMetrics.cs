using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Formatting;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Auth.Queries.GetQrToken;
using SmartSpaces.Application.Features.Users;

namespace SmartSpaces.Application.Features.Identity.Queries.GetMetrics;

public record GetIdentityMetricsQuery() : IRequest<IdentityMetricsDto>;

public record IdentityGrowthDto(
    double ActiveUsers,
    double CredentialsIssuedToday,
    double FailedAttempts,
    double ExpiredCredentials,
    double ActiveSessions,
    double AuthenticationsToday
);

public record IdentityMetricsDto(
    int ActiveUsers,
    int CredentialsIssuedToday,
    int FailedAttempts,
    int ExpiredCredentials,
    int ActiveSessions,
    int AuthenticationsToday,
    IdentityGrowthDto Growth
);

public class GetIdentityMetricsQueryHandler : IRequestHandler<GetIdentityMetricsQuery, IdentityMetricsDto>
{
    private readonly IApplicationDbContext _context;

    public GetIdentityMetricsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IdentityMetricsDto> Handle(GetIdentityMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var todayUtc = now.Date;
        var yesterdayUtc = todayUtc.AddDays(-1);

        // Un QR se emite con vigencia fija, así que "emitido hoy" equivale a que su expiración
        // caiga después del inicio del día más esa vigencia.
        var qrValidity = TimeSpan.FromSeconds(GetQrTokenQueryHandler.ExpiresInSeconds);
        var issuedTodayCutoff = todayUtc.Add(qrValidity);
        var issuedYesterdayCutoff = yesterdayUtc.Add(qrValidity);

        var activeUsers = await _context.Users
            .CountAsync(u => u.Status == UserStatusCatalog.Active, cancellationToken);

        var credentialsIssuedToday = await _context.Users
            .CountAsync(u => u.QrToken != null && u.QrExpiry >= issuedTodayCutoff, cancellationToken);

        // Solo se guarda el último QR de cada usuario, así que el conteo de ayer es un piso
        // (quien haya regenerado hoy ya no cuenta ahí). Sirve para la tendencia, no como censo.
        var credentialsIssuedYesterday = await _context.Users
            .CountAsync(u => u.QrToken != null && u.QrExpiry >= issuedYesterdayCutoff && u.QrExpiry < issuedTodayCutoff, cancellationToken);

        var failedAttempts = await _context.AccessLogs
            .CountAsync(a => !a.Granted && a.Timestamp >= todayUtc, cancellationToken);

        var failedAttemptsYesterday = await _context.AccessLogs
            .CountAsync(a => !a.Granted && a.Timestamp >= yesterdayUtc && a.Timestamp < todayUtc, cancellationToken);

        var expiredCredentials = await _context.Users
            .CountAsync(u => u.QrToken != null && u.QrExpiry < now, cancellationToken);

        // Vigente = marcada como activa y todavía dentro de su ventana de expiración.
        var activeSessions = await _context.Sessions
            .CountAsync(s => s.IsActive && s.ExpiresAt > now, cancellationToken);

        var authenticationsToday = await _context.Sessions
            .CountAsync(s => s.CreatedAt >= todayUtc, cancellationToken);

        var authenticationsYesterday = await _context.Sessions
            .CountAsync(s => s.CreatedAt >= yesterdayUtc && s.CreatedAt < todayUtc, cancellationToken);

        // activeUsers, expiredCredentials y activeSessions son fotos del momento, no flujos:
        // sin una tabla histórica no hay contra qué comparar, y se reportan en 0 antes que inventar.
        var growth = new IdentityGrowthDto(
            ActiveUsers: 0,
            CredentialsIssuedToday: DisplayFormat.GrowthPercentage(credentialsIssuedToday, credentialsIssuedYesterday),
            FailedAttempts: DisplayFormat.GrowthPercentage(failedAttempts, failedAttemptsYesterday),
            ExpiredCredentials: 0,
            ActiveSessions: 0,
            AuthenticationsToday: DisplayFormat.GrowthPercentage(authenticationsToday, authenticationsYesterday)
        );

        return new IdentityMetricsDto(
            activeUsers,
            credentialsIssuedToday,
            failedAttempts,
            expiredCredentials,
            activeSessions,
            authenticationsToday,
            growth);
    }
}
