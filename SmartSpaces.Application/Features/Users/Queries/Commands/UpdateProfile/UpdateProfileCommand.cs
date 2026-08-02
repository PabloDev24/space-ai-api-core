using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(Guid UserId, string? Telefono, string? EmailAlterno) : IRequest<Unit>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    public UpdateProfileCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.Telefono = request.Telefono;
        user.EmailAlterno = request.EmailAlterno;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}