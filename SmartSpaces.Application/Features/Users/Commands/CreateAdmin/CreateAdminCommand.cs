using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Features.Users.Queries.GetUsers;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Features.Users.Commands.CreateAdmin;

/// <summary>
/// Alta de administrador — botón "Agregar Administrador" en Configuración &gt; Usuarios.
/// A diferencia de /api/auth/register, el rol no se recibe del cliente: lo fija el handler.
/// </summary>
public record CreateAdminCommand(
    string Name,
    string Email,
    string Password,
    string? Folio = null
) : IRequest<UserListItemDto>;

public class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand, UserListItemDto>
{
    private const string AdminRole = "admin";

    private readonly IApplicationDbContext _context;

    public CreateAdminCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserListItemDto> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("El correo electrónico ya se encuentra registrado.");
        }

        var folio = string.IsNullOrWhiteSpace(request.Folio) ? null : request.Folio.Trim();

        if (folio != null)
        {
            var folioExists = await _context.Users.AnyAsync(u => u.Folio == folio, cancellationToken);
            if (folioExists)
            {
                throw new InvalidOperationException("El folio institucional ya está asignado a otro usuario.");
            }
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Folio = folio,
            Role = AdminRole,
            Status = UserStatusCatalog.Active
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserListItemDto(admin.Id, admin.Name, admin.Email, admin.Folio, admin.Role, admin.Status);
    }
}
