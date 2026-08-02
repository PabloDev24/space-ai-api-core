using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Users.Queries.GetMyProfile;

public record GetMyProfileQuery(Guid UserId) : IRequest<MyProfileDto>;

public record MyProfileDto(Guid Id, string Nombre, string Matricula, string? Carrera, string? Grupo,
    string? Division, string? Campus, string Email, string? Telefono);

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    private readonly IApplicationDbContext _context;
    public GetMyProfileQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        return new MyProfileDto(user.Id, user.Name, user.Matricula ?? user.Folio ?? "",
            user.Carrera, user.Grupo, user.Division, user.Campus, user.Email, user.Telefono);
    }
}