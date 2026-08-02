using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Users;

namespace SmartSpaces.Application.Features.Identity.Queries.GetActiveSessions;

public record GetActiveSessionsQuery() : IRequest<IReadOnlyList<ActiveSessionDto>>;

public record ActiveSessionDto(
    string Id,
    string Name,
    string Matricula,
    string Role,
    string Status,      // active | expiring | blocked
    string Device,
    string Os,
    DateTime LastAccess,
    DateTime Expiration
);

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, IReadOnlyList<ActiveSessionDto>>
{
    /// <summary>Una sesión a menos de 15 min de expirar se marca como "expiring" en el panel.</summary>
    private static readonly TimeSpan ExpiringThreshold = TimeSpan.FromMinutes(15);

    /// <summary>Login no captura user-agent hoy, así que no hay dispositivo/SO que reportar.</summary>
    private const string UnknownClient = "No registrado";

    private readonly IApplicationDbContext _context;

    public GetActiveSessionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ActiveSessionDto>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var sessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.IsActive && s.ExpiresAt > now)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.CreatedAt,
                s.ExpiresAt,
                UserName = s.User != null ? s.User.Name : null,
                Folio = s.User != null ? s.User.Folio : null,
                Role = s.User != null ? s.User.Role : null,
                UserStatus = s.User != null ? s.User.Status : null
            })
            .ToListAsync(cancellationToken);

        return sessions
            .Select(s => new ActiveSessionDto(
                Id: s.Id.ToString(),
                Name: s.UserName ?? "Usuario desconocido",
                Matricula: string.IsNullOrWhiteSpace(s.Folio) ? "—" : s.Folio,
                Role: s.Role ?? "system",
                Status: ResolveStatus(s.UserStatus, s.ExpiresAt, now),
                Device: UnknownClient,
                Os: UnknownClient,
                LastAccess: s.CreatedAt,
                Expiration: s.ExpiresAt))
            .ToList();
    }

    private static string ResolveStatus(string? userStatus, DateTime expiresAt, DateTime now)
    {
        // Un usuario desactivado mientras tenía sesión abierta aparece como bloqueado,
        // no como activo, aunque su token siga sin expirar.
        if (string.Equals(userStatus, UserStatusCatalog.Inactive, StringComparison.OrdinalIgnoreCase))
        {
            return "blocked";
        }

        return expiresAt - now <= ExpiringThreshold ? "expiring" : "active";
    }
}
