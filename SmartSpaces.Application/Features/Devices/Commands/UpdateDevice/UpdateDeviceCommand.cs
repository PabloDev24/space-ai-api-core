using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Devices.Queries.GetDevices;

namespace SmartSpaces.Application.Features.Devices.Commands.UpdateDevice;

/// <summary>Edición de un dispositivo existente desde la cuadrícula del panel.</summary>
public record UpdateDeviceCommand(
    Guid Id,
    string Code,
    string Name,
    string Type,
    string? Location = null,
    string? Status = null
) : IRequest<DeviceDto>;

public class UpdateDeviceCommandHandler : IRequestHandler<UpdateDeviceCommand, DeviceDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceDto> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (device == null)
        {
            throw new KeyNotFoundException("Dispositivo no encontrado.");
        }

        var code = request.Code.Trim();

        var codeTaken = await _context.Devices
            .AnyAsync(d => d.Id != request.Id && d.Code.ToLower() == code.ToLower(), cancellationToken);

        if (codeTaken)
        {
            throw new InvalidOperationException($"Ya existe otro dispositivo con el código '{code}'.");
        }

        var previousStatus = device.Status;

        device.Code = code;
        device.Name = request.Name.Trim();
        device.Type = request.Type.ToUpperInvariant();
        device.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            device.Status = request.Status.ToUpperInvariant();
        }

        // LastSeen refleja el último reporte del dispositivo, no la edición del registro;
        // solo se refresca cuando el estado cambia (es decir, cuando sí hubo señal de vida).
        if (device.Status != previousStatus)
        {
            device.LastSeen = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new DeviceDto(device.Id, device.Code, device.Name, device.Type, device.Status, device.Location, device.LastSeen);
    }
}
