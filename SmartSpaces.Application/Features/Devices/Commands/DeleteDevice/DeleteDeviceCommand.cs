using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Devices.Commands.DeleteDevice;

/// <summary>Baja de un dispositivo de la cuadrícula del panel.</summary>
public record DeleteDeviceCommand(Guid Id) : IRequest<bool>;

public class DeleteDeviceCommandHandler : IRequestHandler<DeleteDeviceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (device == null)
        {
            throw new KeyNotFoundException("Dispositivo no encontrado.");
        }

        // Si el lector está dado de alta como punto de acceso, primero hay que quitarlo de ahí:
        // borrarlo en silencio dejaría la vista de Puntos de Acceso apuntando a hardware inexistente.
        var linkedAccessPoint = await _context.AccessPoints
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.DeviceId.ToLower() == device.Code.ToLower(), cancellationToken);

        if (linkedAccessPoint != null)
        {
            throw new InvalidOperationException(
                $"El dispositivo está asignado al punto de acceso '{linkedAccessPoint.Name}'. Elimina o reasigna el punto de acceso primero.");
        }

        // Los AccessLog guardan el DeviceId como texto (sin FK), así que el histórico de escaneos
        // sobrevive a la baja del dispositivo — es intencional: es evidencia de auditoría.
        _context.Devices.Remove(device);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
