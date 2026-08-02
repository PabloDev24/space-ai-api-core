using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Formatting;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Access.Queries.GetPoints;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Features.Access.Commands.CreatePoint;

/// <summary>Alta de torniquetes / plumas / puertas desde la vista de Puntos de Acceso.</summary>
public record CreateAccessPointCommand(
    string Name,
    string Building,
    string DeviceId,
    string? Status = null,
    int? NetworkPingMs = null
) : IRequest<AccessPointDto>;

public class CreateAccessPointCommandHandler : IRequestHandler<CreateAccessPointCommand, AccessPointDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAccessPointCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessPointDto> Handle(CreateAccessPointCommand request, CancellationToken cancellationToken)
    {
        var deviceId = request.DeviceId.Trim();

        // Un mismo lector no puede estar dado de alta dos veces: los escaneos se atribuyen
        // por DeviceId y quedarían contados en dos puntos a la vez.
        var deviceIdTaken = await _context.AccessPoints
            .AnyAsync(p => p.DeviceId.ToLower() == deviceId.ToLower(), cancellationToken);

        if (deviceIdTaken)
        {
            throw new InvalidOperationException($"Ya existe un punto de acceso registrado con el lector '{deviceId}'.");
        }

        var point = new AccessPoint
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Building = request.Building.Trim(),
            DeviceId = deviceId,
            Status = AccessPointCatalog.NormalizeStatus(request.Status) ?? AccessPointCatalog.Active,
            NetworkPingMs = request.NetworkPingMs ?? 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccessPoints.Add(point);
        await _context.SaveChangesAsync(cancellationToken);

        // Recién creado no tiene escaneos todavía: se devuelve con los contadores en cero
        // para que el panel pueda insertarlo en la lista sin recargar.
        return new AccessPointDto(
            point.Id,
            point.Name,
            point.Building,
            ScansToday: 0,
            LastValidation: DisplayFormat.RelativeTime(null, emptyLabel: "Sin validaciones"),
            point.NetworkPingMs,
            point.Status);
    }
}
