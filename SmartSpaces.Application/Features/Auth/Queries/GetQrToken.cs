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

    public async Task<QrTokenResponse> Handle(GetQrTokenQuery request, CancellationToken cancellationToken)
    {
        // Validamos que el alumno exista de verdad en la DB
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("El alumno especificado no existe.");
        }

        // Generamos el token dinámico encriptado
        var qrToken = _qrCodeService.GenerateEncryptedQrToken(user.Id, user.Role);

        // 15 minutos = 900 segundos
        return new QrTokenResponse(qrToken, 900);
    }
}