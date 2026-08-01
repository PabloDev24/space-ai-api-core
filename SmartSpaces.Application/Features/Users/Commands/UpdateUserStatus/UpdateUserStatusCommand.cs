using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Users.Queries.GetUsers;

namespace SmartSpaces.Application.Features.Users.Commands.UpdateUserStatus;

/// <summary>Switch "Activar/Desactivar Usuario" de la tabla de usuarios.</summary>
public record UpdateUserStatusCommand(Guid Id, string Status) : IRequest<UserListItemDto>;

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, UserListItemDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserListItemDto> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        var status = UserStatusCatalog.Normalize(request.Status)
            ?? throw new InvalidOperationException($"El estado debe ser uno de: {string.Join(", ", UserStatusCatalog.All)}.");

        user.Status = status;

        // Desactivar debe cortar el acceso ya concedido, no solo el próximo login: sin esto el
        // usuario seguiría entrando con la sesión que ya tenía abierta hasta que expirara sola.
        if (status == UserStatusCatalog.Inactive)
        {
            var openSessions = await _context.Sessions
                .Where(s => s.UserId == user.Id && s.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var session in openSessions)
            {
                session.IsActive = false;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new UserListItemDto(user.Id, user.Name, user.Email, user.Folio, user.Role, user.Status);
    }
}
