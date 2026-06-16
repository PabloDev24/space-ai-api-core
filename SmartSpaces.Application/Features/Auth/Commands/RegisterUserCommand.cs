using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Domain.Entities;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Auth.Commands;

// El Request que recibe MediatR (igual al Body del contrato)
public record RegisterUserCommand(
    string Name,
    string Email,
    string Password,
    string? Folio,
    string Role
) : IRequest<RegisterUserResponse>;

// El DTO de respuesta que exige el contrato
public record RegisterUserResponse(string Message, Guid UserId);

// El Handler que ejecuta la lógica de negocio
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IApplicationDbContext _context;

    public RegisterUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar si el email ya existe
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
            throw new Exception("El correo electrónico ya se encuentra registrado.");

        // 2. Validar si el folio ya existe (solo si no es visitante/nulo)
        if (!string.IsNullOrEmpty(request.Folio))
        {
            var folioExists = await _context.Users.AnyAsync(u => u.Folio == request.Folio, cancellationToken);
            if (folioExists)
                throw new Exception("El folio institucional ya está asignado a otro usuario.");
        }

        // 3. Hashear la contraseña de forma segura con BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 4. Crear la entidad de dominio
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            Folio = request.Folio,
            Role = request.Role.ToLower()
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse("Usuario registrado exitosamente", newUser.Id);
    }
}