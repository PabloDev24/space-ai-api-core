using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Auth.Queries.GetQrToken;

public record GetQrTokenQuery(Guid UserId) : IRequest<QrTokenResponse>;
public record QrTokenResponse(string QrToken, int ExpiresInSeconds);

public class GetQrTokenQueryHandler : IRequestHandler<GetQrTokenQuery, QrTokenResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public GetQrTokenQueryHandler(IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
    }

    /// <summary>Vigencia del QR en segundos (15 min).</summary>
    public const int ExpiresInSeconds = 900;

    public async Task<QrTokenResponse> Handle(GetQrTokenQuery request, CancellationToken cancellationToken)
    {
        // Validamos que el alumno exista de verdad en la DB
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("El alumno especificado no existe.");
        }

        // Generamos el token dinámico encriptado
        var qrToken = _qrCodeService.GenerateEncryptedQrToken(user.Id, user.Role);

        // Dejamos rastro de la emisión: el token en sí es autocontenido (AES) y no necesita la DB
        // para validarse, pero sin esto no hay forma de reportar credenciales emitidas/expiradas
        // en /api/identity/metrics — las columnas QrToken/QrExpiry existían sin llenarse nunca.
        user.QrToken = qrToken;
        user.QrExpiry = DateTime.UtcNow.AddSeconds(ExpiresInSeconds);
        await _context.SaveChangesAsync(cancellationToken);

        return new QrTokenResponse(qrToken, ExpiresInSeconds);
    }
}