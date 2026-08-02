using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Dashboard.Queries.GetNextClass;

public record GetNextClassQuery(Guid UserId) : IRequest<NextClassDto?>;
public record NextClassDto(string NombreMateria, string Hora, string Edificio, string Salon);

public class GetNextClassQueryHandler : IRequestHandler<GetNextClassQuery, NextClassDto?>
{
    private readonly IApplicationDbContext _context;
    public GetNextClassQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<NextClassDto?> Handle(GetNextClassQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now; // considera manejar zona horaria del campus explícitamente
        var clase = await _context.ClasesHorario.AsNoTracking()
            .Include(c => c.Materia)
            .Where(c => c.DiaSemana == now.DayOfWeek && c.HoraInicio >= now.TimeOfDay)
            .OrderBy(c => c.HoraInicio)
            .FirstOrDefaultAsync(cancellationToken);

        return clase == null ? null : new NextClassDto(clase.Materia!.Nombre, clase.HoraInicio.ToString(@"hh\:mm"), clase.Edificio, clase.Salon);
    }
}