using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Devices.Queries.GetDevices;

public record GetDevicesQuery(string? Type = null, string? Status = null) : IRequest<IReadOnlyList<DeviceDto>>;

public record DeviceDto(
    Guid Id,
    string Code,
    string Name,
    string Type,
    string Status,
    string? Location,
    DateTime LastSeen
);

public class GetDevicesQueryHandler : IRequestHandler<GetDevicesQuery, IReadOnlyList<DeviceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDevicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DeviceDto>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Devices.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var type = request.Type.ToUpper();
            query = query.Where(d => d.Type.ToUpper() == type);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.ToUpper();
            query = query.Where(d => d.Status.ToUpper() == status);
        }

        return await query
            .OrderBy(d => d.Name)
            .Select(d => new DeviceDto(d.Id, d.Code, d.Name, d.Type, d.Status, d.Location, d.LastSeen))
            .ToListAsync(cancellationToken);
    }
}
