using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Auth.Commands.ValidateQr;

public record ValidateQrCommand(string QrToken) : IRequest<QrValidationResponse>;
public record QrValidationResponse(bool IsValid, string Message, ValidatedUserDto? User);
public record ValidatedUserDto(Guid Id, string Name, string Email, string? Folio, string Role);

public class ValidateQrCommandHandler : IRequestHandler<ValidateQrCommand, QrValidationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public ValidateQrCommandHandler(IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<QrValidationResponse> Handle(ValidateQrCommand request, CancellationToken cancellationToken)
    {
        // 1. Desencriptar el QR con AES
        var qrData = _qrCodeService.ValidateAndDecryptQrToken(request.QrToken);

        if (qrData == null)
        {
            return new QrValidationResponse(false, "QR Inválido o alterado criptográficamente.", null);
        }

        // 2. Verificar expiración estricta de 15 minutos
        if (DateTime.UtcNow > qrData.ExpiredAt)
        {
            return new QrValidationResponse(false, "El código QR ha expirado. Solicite uno nuevo.", null);
        }

        // 3. Buscar el perfil actual en PostgreSQL para retornar su contexto
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == qrData.UserId, cancellationToken);

        if (user == null)
        {
            return new QrValidationResponse(false, "El usuario asociado al QR ya no existe.", null);
        }

        // Acceso exitoso, devolvemos el perfil completo que necesita el torniquete
        var userDto = new ValidatedUserDto(user.Id, user.Name, user.Email, user.Folio, user.Role);
        return new QrValidationResponse(true, "Acceso Autorizado.", userDto);
    }
}