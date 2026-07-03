using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Features.Access.Commands.Scan;

public record ScanAccessCommand(string QrToken, string DeviceId, string Direction) : IRequest<ScanAccessResult>;

public record ScanAccessUserDto(Guid Id, string Name, string? Folio, string Role);

public record ScanAccessResult(bool Granted, string Message, ScanAccessUserDto? User, DateTime Timestamp);

public class ScanAccessCommandHandler : IRequestHandler<ScanAccessCommand, ScanAccessResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public ScanAccessCommandHandler(IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<ScanAccessResult> Handle(ScanAccessCommand request, CancellationToken cancellationToken)
    {
        var direction = request.Direction?.ToUpperInvariant() == "OUT" ? "OUT" : "IN";

        // 1. Validar y desencriptar el QR (reutiliza la lógica AES del módulo Auth).
        var qrData = _qrCodeService.ValidateAndDecryptQrToken(request.QrToken);
        if (qrData == null)
        {
            return new ScanAccessResult(false, "QR inválido o alterado.", null, DateTime.UtcNow);
        }

        if (DateTime.UtcNow > qrData.ExpiredAt)
        {
            return new ScanAccessResult(false, "El código QR ha expirado.", null, DateTime.UtcNow);
        }

        // 2. Verificar que el usuario siga existiendo.
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == qrData.UserId, cancellationToken);

        if (user == null)
        {
            return new ScanAccessResult(false, "El usuario asociado al QR ya no existe.", null, DateTime.UtcNow);
        }

        // 3. Registrar el acceso concedido.
        var timestamp = DateTime.UtcNow;
        await LogAccessAsync(user.Id, request.DeviceId, direction, true, timestamp, cancellationToken);

        var message = direction == "OUT" ? "Salida registrada." : "Acceso permitido.";
        var userDto = new ScanAccessUserDto(user.Id, user.Name, user.Folio, user.Role);
        return new ScanAccessResult(true, message, userDto, timestamp);
    }

    private async Task LogAccessAsync(Guid userId, string deviceId, string direction, bool granted, DateTime timestamp, CancellationToken cancellationToken)
    {
        try
        {
            _context.AccessLogs.Add(new AccessLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceId = deviceId,
                Direction = direction,
                Granted = granted,
                Timestamp = timestamp,
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Registrar el evento no debe romper la respuesta al torniquete (docs/00 regla 8).
        }
    }
}
