using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Auth.Queries;

// Request del Login
public record LoginQuery(string Email, string Password) : IRequest<LoginResponse>;

// DTO de respuesta estructurado al Sprint 1
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserDto User
);

public record UserDto(Guid Id, string Name, string Email, string? Folio, string Role);

public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponse>
{
    private readonly IApplicationDbContext _context;

    public LoginQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        // 1. Buscar al usuario por Email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // 2. Validar existencia y verificar hash de la contraseña
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Lanzamos una excepción que capturará nuestro GlobalExceptionHandler (401 / 400)
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        // 3. Mock temporal de tokens (En la siguiente HU montaremos JWT real)
        var temporaryAccessToken = "eyJhbGciOiJIUzI1NiIsInR5...";
        var temporaryRefreshToken = Guid.NewGuid().ToString();

        var userDto = new UserDto(user.Id, user.Name, user.Email, user.Folio, user.Role);

        return new LoginResponse(temporaryAccessToken, temporaryRefreshToken, 3600, userDto);
    }
}