using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Auth.Queries.GetQrToken;

namespace SmartSpaces.Application.Features.Identity.Queries.GetActivity;

public record GetIdentityActivityQuery(int Limit = 20) : IRequest<IReadOnlyList<IdentityEventDto>>;

/// <summary>
/// Type:   SUCCESS | QR_GENERATED | FAILED | EXPIRED (RENEWED y LOGOUT no se emiten hoy:
///         no existe endpoint de refresh ni de logout que los produzca).
/// Status: valid | error | warning | info — define el color del ícono en el panel.
/// </summary>
public record IdentityEventDto(
    string Id,
    string Type,
    string Description,
    DateTime Timestamp,
    string Status,
    string? User
);

public class GetIdentityActivityQueryHandler : IRequestHandler<GetIdentityActivityQuery, IReadOnlyList<IdentityEventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetIdentityActivityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<IdentityEventDto>> Handle(GetIdentityActivityQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit is > 0 and <= 100 ? request.Limit : 20;
        var now = DateTime.UtcNow;

        // Cada fuente aporta a lo más `limit` eventos; al final se mezclan y se recortan.
        // Traer más sería desperdicio: ninguno puede aportar más de limit al resultado final.
        var sessions = await _context.Sessions
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .Select(s => new
            {
                s.Id,
                s.CreatedAt,
                s.ExpiresAt,
                UserName = s.User != null ? s.User.Name : null,
                Folio = s.User != null ? s.User.Folio : null
            })
            .ToListAsync(cancellationToken);

        var accessLogs = await _context.AccessLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.Timestamp,
                a.Granted,
                a.DeviceId,
                UserName = a.User != null ? a.User.Name : null,
                Folio = a.User != null ? a.User.Folio : null
            })
            .ToListAsync(cancellationToken);

        var qrValidity = TimeSpan.FromSeconds(GetQrTokenQueryHandler.ExpiresInSeconds);

        var issuedCredentials = await _context.Users
            .AsNoTracking()
            .Where(u => u.QrToken != null)
            .OrderByDescending(u => u.QrExpiry)
            .Take(limit)
            .Select(u => new { u.Id, u.Name, u.Folio, u.QrExpiry })
            .ToListAsync(cancellationToken);

        var events = new List<IdentityEventDto>();

        foreach (var session in sessions)
        {
            events.Add(new IdentityEventDto(
                Id: $"session-{session.Id}",
                Type: "SUCCESS",
                Description: "Inicio de sesión exitoso",
                Timestamp: session.CreatedAt,
                Status: "valid",
                User: FormatUser(session.UserName, session.Folio)));

            if (session.ExpiresAt <= now)
            {
                events.Add(new IdentityEventDto(
                    Id: $"session-expired-{session.Id}",
                    Type: "EXPIRED",
                    Description: "Sesión expirada automáticamente",
                    Timestamp: session.ExpiresAt,
                    Status: "warning",
                    User: FormatUser(session.UserName, session.Folio)));
            }
        }

        foreach (var log in accessLogs)
        {
            events.Add(log.Granted
                ? new IdentityEventDto(
                    Id: $"access-{log.Id}",
                    Type: "SUCCESS",
                    Description: $"Credencial validada en {log.DeviceId}",
                    Timestamp: log.Timestamp,
                    Status: "valid",
                    User: FormatUser(log.UserName, log.Folio))
                : new IdentityEventDto(
                    Id: $"access-{log.Id}",
                    Type: "FAILED",
                    Description: $"Acceso rechazado en {log.DeviceId}",
                    Timestamp: log.Timestamp,
                    Status: "error",
                    User: FormatUser(log.UserName, log.Folio)));
        }

        foreach (var credential in issuedCredentials)
        {
            events.Add(new IdentityEventDto(
                Id: $"qr-{credential.Id}",
                Type: "QR_GENERATED",
                Description: "Nueva credencial QR generada",
                Timestamp: credential.QrExpiry - qrValidity,
                Status: "info",
                User: FormatUser(credential.Name, credential.Folio)));
        }

        return events
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();
    }

    private static string FormatUser(string? name, string? folio)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Usuario desconocido";
        }

        return string.IsNullOrWhiteSpace(folio) ? name : $"{name} · {folio}";
    }
}
