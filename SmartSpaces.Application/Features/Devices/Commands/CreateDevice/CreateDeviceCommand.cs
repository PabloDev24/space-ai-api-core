using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Devices.Queries.GetDevices;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Features.Devices.Commands.CreateDevice;

/// <summary>Alta de dispositivo — botón "Registrar Dispositivo" del panel.</summary>
public record CreateDeviceCommand(
    string Code,
    string Name,
    string Type,
    string? Location = null,
    string? Status = null
) : IRequest<DeviceDto>;

public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, DeviceDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceDto> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();

        // Code tiene índice único en la DB: validamos antes para devolver un mensaje claro
        // en lugar de una excepción de Postgres.
        var codeExists = await _context.Devices
            .AnyAsync(d => d.Code.ToLower() == code.ToLower(), cancellationToken);

        if (codeExists)
        {
            throw new InvalidOperationException($"Ya existe un dispositivo con el código '{code}'.");
        }

        var device = new Device
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            Type = request.Type.ToUpperInvariant(),
            // Un dispositivo recién dado de alta todavía no reporta: nace OFFLINE salvo que
            // se indique lo contrario explícitamente.
            Status = string.IsNullOrWhiteSpace(request.Status) ? "OFFLINE" : request.Status.ToUpperInvariant(),
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            LastSeen = DateTime.UtcNow
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeviceDto(device.Id, device.Code, device.Name, device.Type, device.Status, device.Location, device.LastSeen);
    }
}
