using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Academic.Queries.GetSchedule;

public record GetScheduleQuery(Guid UserId) : IRequest<IReadOnlyList<ScheduleItemDto>>;

public record ScheduleItemDto(Guid ClaseId, string Materia, string HoraInicio, string HoraFin,
    string Edificio, string Salon, string DiaSemana);

public class GetScheduleQueryHandler : IRequestHandler<GetScheduleQuery, IReadOnlyList<ScheduleItemDto>>
{
    private readonly IApplicationDbContext _context;
    public GetScheduleQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<ScheduleItemDto>> Handle(GetScheduleQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (string.IsNullOrEmpty(user.Grupo))
            return Array.Empty<ScheduleItemDto>();

        return await _context.ClasesHorario.AsNoTracking()
            .Include(c => c.Materia)
            .Where(c => c.Grupo == user.Grupo)
            .OrderBy(c => c.DiaSemana).ThenBy(c => c.HoraInicio)
            .Select(c => new ScheduleItemDto(c.Id, c.Materia!.Nombre, c.HoraInicio.ToString(@"hh\:mm"),
                c.HoraFin.ToString(@"hh\:mm"), c.Edificio, c.Salon, c.DiaSemana.ToString()))
            .ToListAsync(cancellationToken);
    }
}