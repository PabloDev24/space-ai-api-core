using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Academic.Queries.GetGrades;

public record GetGradesQuery(Guid UserId) : IRequest<IReadOnlyList<GradeDto>>;
public record GradeDto(Guid MateriaId, string Nombre, string Profesor, double? Parcial1, double? Parcial2, double? Parcial3, double? Final);

public class GetGradesQueryHandler : IRequestHandler<GetGradesQuery, IReadOnlyList<GradeDto>>
{
    private readonly IApplicationDbContext _context;
    public GetGradesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<GradeDto>> Handle(GetGradesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Calificaciones.AsNoTracking()
            .Where(c => c.UserId == request.UserId)
            .Include(c => c.Materia)
            .Select(c => new GradeDto(c.MateriaId, c.Materia!.Nombre, c.Materia!.Profesor, c.Parcial1, c.Parcial2, c.Parcial3, c.Final))
            .ToListAsync(cancellationToken);
    }
}