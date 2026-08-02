using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Academic.Queries.GetSummary;

public record GetAcademicSummaryQuery(Guid UserId) : IRequest<AcademicSummaryDto>;
public record AcademicSummaryDto(double Gpa, int TotalAttendance, string PeriodoActual);

public class GetAcademicSummaryQueryHandler : IRequestHandler<GetAcademicSummaryQuery, AcademicSummaryDto>
{
    private readonly IApplicationDbContext _context;
    public GetAcademicSummaryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<AcademicSummaryDto> Handle(GetAcademicSummaryQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var finales = await _context.Calificaciones.AsNoTracking()
            .Where(c => c.UserId == request.UserId && c.Final != null)
            .Select(c => c.Final!.Value)
            .ToListAsync(cancellationToken);

        var gpa = finales.Count > 0 ? Math.Round(finales.Average(), 2) : 0;

        return new AcademicSummaryDto(gpa, user.TotalAttendance, "2026-2"); // ajusta el periodo a tu lógica real
    }
}